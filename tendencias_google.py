# -*- coding: utf-8 -*-

import sys
import json
import re
import time
from datetime import datetime
from playwright.sync_api import sync_playwright
import pandas as pd

def convertir_volumen(volumen_texto: str) -> int:
    """
    Convierte texto como '200 mil+' o '1 millon+' a numero entero.
    """
    if not volumen_texto:
        return 0

    texto = volumen_texto.lower().replace("+", "").strip()
    numero = re.findall(r"[\d.,]+", texto)
    if not numero:
        return 0

    valor_str = numero[0]

    if "." in valor_str and "," in valor_str:
        valor_str = valor_str.replace(".", "").replace(",", ".")
    elif "." in valor_str:
        if re.match(r"^\d{1,3}\.\d{3}$", valor_str):
            valor_str = valor_str.replace(".", "")
        else:
            valor_str = valor_str.replace(",", ".")
    elif "," in valor_str:
        if re.match(r"^\d{1,3},\d{3}$", valor_str):
            valor_str = valor_str.replace(",", "")
        else:
            valor_str = valor_str.replace(",", ".")

    try:
        valor = float(valor_str)
    except ValueError:
        return 0

    if re.search(r"\bmil\b", texto):
        valor *= 1_000
    elif re.search(r"\bmill[oó]n(?:es)?\b", texto):
        valor *= 1_000_000

    return int(valor)


def obtener_tendencias_google():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        url = "https://trends.google.es/trending?geo=PE&hours=168"
        page.goto(url, timeout=60000)

        page.wait_for_selector('tr[role="row"]', timeout=60000)
        time.sleep(3)

        filas = page.locator('tr[role="row"]')
        total = filas.count()

        datos = []
        fecha_consulta = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        for i in range(total):
            try:
                fila = filas.nth(i)
                celdas = fila.locator("td")
                if celdas.count() < 3:
                    continue

                tendencia = celdas.nth(1).inner_text(timeout=1500).split("\n")[0].strip()
                volumen_texto = celdas.nth(2).inner_text(timeout=1500).split("\n")[0].strip()
                volumen_num = convertir_volumen(volumen_texto)

                if tendencia and volumen_num > 0:
                    datos.append({
                        "tendencia": tendencia,
                        "volumen": volumen_num,
                        "fecha_consulta": fecha_consulta
                    })
            except Exception:
                continue

        browser.close()

        return datos


if __name__ == "__main__":
    try:
        resultados = obtener_tendencias_google()

        if not resultados:
            print(json.dumps({"error": "No se obtuvieron tendencias válidas"}))
        else:
            # Ordenar por volumen descendente y limitar a top 20
            df = pd.DataFrame(resultados)
            df = df.sort_values(by="volumen", ascending=False).head(20)
            salida = df.to_dict(orient="records")
            print(json.dumps(salida, ensure_ascii=False))
    except Exception as e:
        print(json.dumps({"error": str(e)}))