import sys
import os
import pdfplumber
import PIL
import io
import pymupdf
import pytesseract
import cv2
import numpy as np
import json

from PIL import Image

pytesseract.pytesseract.tesseract_cmd = r"TessOCR/tesseract.exe"
os.environ['TESSDATA_PREFIX'] = r"TessOCR/tessdata"

#pymudf4llm
#tesseract ocr

class PDF_extractor:
    def __init__(self):
        pytesseract.pytesseract.tesseract_cmd = r"TessOCR/tesseract.exe"
        os.environ['TESSDATA_PREFIX'] = r"TessOCR/tessdata"

    def get_text_pdfplumber(self, filename):
        text_from_pages = []
        with pdfplumber.open(filename) as pdf_file:
            for page in pdf_file.pages:
                text_from_pages.append(page.extract_text())
        return text_from_pages

    def get_text_pymupdf(self, filename):
        text_from_pages = []
        with pymupdf.open(filename) as pdf_file:
            for page in pdf_file:
                text_from_pages.append(page.get_text())
        return text_from_pages

    def preprocess_image(self, pil_image):
        # Конвертируем в numpy array
        img = np.array(pil_image.convert('L'))  # в оттенки серого
        # Бинаризация (порог Otsu)
        _, binary = cv2.threshold(img, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        # Удаление шума
        denoised = cv2.medianBlur(binary, 3)
        return Image.fromarray(denoised)

    def get_images_pymupdf(self, filename):
        text_from_pages = []
        with pymupdf.open(filename) as pdf_file:
            for page_num, page in enumerate(pdf_file, start=1):
                for img_index, img in enumerate(page.get_images()):
                    if img[2] < 200 or img[3] < 200:
                        continue
                    xref = img[0]
                    base_image = pdf_file.extract_image(xref)
                    image_bytes = base_image["image"]
                    pil_image = Image.open(io.BytesIO(image_bytes))
                    if pil_image.mode not in ('L', 'RGB'):
                        pil_image = pil_image.convert('RGB')
                    # Предобработка и OCR
                    try:
                        processed = self.preprocess_image(pil_image)
                        ocr_text = pytesseract.image_to_string(processed, lang="rus")
                        if ocr_text.strip():
                            text_from_pages.append(ocr_text)
                            #print(f"--- Страница {page_num}, изображение {img_index+1} ---\n{ocr_text}\n")
                    except Exception as e:
                        print(f"Ошибка OCR на стр.{page_num}: {e}")
        return text_from_pages

if __name__ == "__main__":
    extractor = PDF_extractor()
    input_file = sys.argv[1]
    result = extractor.get_images_pymupdf(input_file)
    print(json.dumps(result, ensure_ascii=False))