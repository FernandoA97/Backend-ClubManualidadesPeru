# -*- coding: utf-8 -*-
from playwright.sync_api import sync_playwright
import pandas as pd
from datetime import datetime
import time
import re
import json

def scrape_lanapolis():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        url = "https://lanapolis.pe/c/hilos-y-lanas"
        page.goto(url, timeout=60000)
        page.wait_for_selector("article.product-card", timeout=60000)
        time.sleep(3)

        productos = []
        cards = page.locator("article.product-card")
        total = cards.count()

        for i in range(total):
            try:
                card = cards.nth(i)

                nombre = card.locator("h3.card-name a").first.inner_text().strip()

                precio_texto = card.locator(".card-price").first.inner_text().strip()
                precio_texto = re.sub(r"[^\d.,]", "", precio_texto)
                precio = float(precio_texto.replace(",", "."))

                imagen = card.locator("img").first.get_attribute("src")

                productos.append({
                    "nombre": nombre,
                    "precio": precio,
                    "imagen": imagen,
                    "fecha_consulta": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                })

            except Exception:
                continue

        browser.close()
        return productos

if __name__ == "__main__":
    try:
        resultados = scrape_lanapolis()

        if not resultados:
            print(json.dumps({"error": "No se encontraron productos"}))
        else:
            df = pd.DataFrame(resultados)
            df = df.sort_values(by="precio", ascending=True)
            salida = df.to_dict(orient="records")
            print(json.dumps(salida, ensure_ascii=False))
    except Exception as e:
        print(json.dumps({"error": str(e)}))
