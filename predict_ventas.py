import sys
import json
import pandas as pd
import os
import joblib
from sklearn.ensemble import RandomForestRegressor
from datetime import timedelta, datetime
from calendar import monthrange

file_path = sys.argv[1]
horizonte_prediccion = sys.argv[2] if len(sys.argv) > 2 else '7dias'

with open(file_path, "r") as f:
    ventas = json.load(f)

df = pd.DataFrame(ventas)
df['FechaDeMision'] = pd.to_datetime(df['FechaDeMision'], errors='coerce')

df = df.dropna(subset=['FechaDeMision', 'MontoTotal'])
if df.empty:
    print(json.dumps({"error": "No hay fechas válidas para procesar"}))
    sys.exit(0)


df_agg = df.groupby('FechaDeMision').agg({'MontoTotal':'sum'}).reset_index()


df_agg['Dia'] = df_agg['FechaDeMision'].dt.day
df_agg['Mes'] = df_agg['FechaDeMision'].dt.month
df_agg['Anio'] = df_agg['FechaDeMision'].dt.year
df_agg['DiaSemana'] = df_agg['FechaDeMision'].dt.weekday


df_agg['MontoPrevio1'] = df_agg['MontoTotal'].shift(1).bfill()
df_agg['MontoPrevio3'] = df_agg['MontoTotal'].rolling(3).mean().shift(1).bfill()


X = df_agg[['Anio','Mes','Dia','DiaSemana','MontoPrevio1','MontoPrevio3']]
y = df_agg['MontoTotal']


modelo_file = "modelo_ventas.pkl"
if os.path.exists(modelo_file):
    modelo = joblib.load(modelo_file)
else:
    modelo = RandomForestRegressor(n_estimators=200, random_state=42)
    modelo.fit(X, y)
    joblib.dump(modelo, modelo_file)

hoy = df_agg['FechaDeMision'].max() + timedelta(days=1)
fechas_futuras = []

if horizonte_prediccion == '7dias':
    fechas_futuras = [hoy + timedelta(days=i) for i in range(7)]
elif horizonte_prediccion == '30dias':
    fechas_futuras = [hoy + timedelta(days=i) for i in range(30)]
elif horizonte_prediccion == '1mes':
    next_month = (hoy.month % 12) + 1
    year = hoy.year if next_month > 1 else hoy.year + 1
    days_in_month = monthrange(year, next_month)[1]
    fechas_futuras = [datetime(year, next_month, d) for d in range(1, days_in_month+1)]
elif horizonte_prediccion == '3meses':
    for m in range(3):
        mes = (hoy.month + m - 1) % 12 + 1
        year = hoy.year + ((hoy.month + m - 1) // 12)
        days_in_month = monthrange(year, mes)[1]
        fechas_futuras.extend([datetime(year, mes, d) for d in range(1, days_in_month+1)])
elif horizonte_prediccion == '1año':

    for m in range(1, 13):
        fechas_futuras.append(datetime(hoy.year + 1, m, 1))


ultimo_monto = df_agg['MontoTotal'].iloc[-1]
ultimo_prom3 = df_agg['MontoTotal'].iloc[-3:].mean()

X_pred = pd.DataFrame({
    'Anio':[d.year for d in fechas_futuras],
    'Mes':[d.month for d in fechas_futuras],
    'Dia':[d.day for d in fechas_futuras],
    'DiaSemana':[d.weekday() for d in fechas_futuras],
    'MontoPrevio1':[ultimo_monto]*len(fechas_futuras),
    'MontoPrevio3':[ultimo_prom3]*len(fechas_futuras)
})

predicciones = modelo.predict(X_pred)


if horizonte_prediccion == '1año':
  
    df_pred = pd.DataFrame({'fecha':[d.strftime("%Y-%m") for d in fechas_futuras], 'prediccion':predicciones})
    df_pred = df_pred.groupby('fecha').sum().reset_index()
    salida = df_pred.to_dict(orient='records')
else:
    salida = [{"fecha": d.strftime("%Y-%m-%d"), "prediccion": float(p)} 
              for d, p in zip(fechas_futuras, predicciones)]

print(json.dumps(salida))
