# -*- coding: utf-8 -*-
from playwright.sync_api import sync_playwright
import pandas as pd
from datetime import datetime
import time
import re
import json
import sys

def limpiar_precio(precio_texto: str) -> float:
    if not precio_texto:
        return 0.0

    precio_texto = precio_texto.replace("S/.", "").replace("S/", "").replace("S.", "").strip()
    precio_texto = re.sub(r"[^0-9,\.]", "", precio_texto)
    precio_texto = re.sub(r"^\.+", "", precio_texto)
    precio_texto = precio_texto.replace(",", ".")

    try:
        return float(precio_texto)
    except ValueError:
        return 0.0


def scrape_entrelanas():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        url = "https://entrelanas.pe/collections/tejidos"
        page.goto(url, timeout=60000)
        page.wait_for_selector("div.tt-product", timeout=60000)
        time.sleep(3)

        productos = []
        cards = page.locator("div.tt-product")
        total = cards.count()

        for i in range(total):
            try:
                card = cards.nth(i)
                nombre = card.locator("h2.tt-title a").inner_text().strip()
                precio_texto = card.locator(".tt-price span").first.inner_text().strip()
                precio = limpiar_precio(precio_texto)

                imagen = card.locator("ul.tt-options-swatch li.active").get_attribute("data-img")
                if not imagen:
                    imagen = (
                        card.locator("img").first.get_attribute("data-mainimage")
                        or card.locator("img").first.get_attribute("src")
                    )

                if imagen:
                    if imagen.startswith("//"):
                        imagen = "https:" + imagen
                    elif imagen.startswith("/"):
                        imagen = "https://entrelanas.pe" + imagen
                    elif not imagen.startswith("http"):
                        imagen = "https://entrelanas.pe/" + imagen.lstrip("/")

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
        resultados = scrape_entrelanas()

        if not resultados:
            print(json.dumps({"error": "No se encontraron productos"}))
        else:
            df = pd.DataFrame(resultados)
            df = df.sort_values(by="precio", ascending=True)
            salida = df.to_dict(orient="records")
            sys.stdout.write(json.dumps(salida, ensure_ascii=False))
            sys.stdout.flush()
    except Exception as e:
        sys.stdout.write(json.dumps({"error": str(e)}))
        sys.stdout.flush()
