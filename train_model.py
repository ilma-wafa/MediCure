import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType
import numpy as np

# Load dataset
df = pd.read_csv('KaggleV2-May-2016.csv')

print("Dataset loaded. Shape:", df.shape)
print(df.head())

# Clean column names
df.columns = df.columns.str.strip()

# Feature engineering
df['Age'] = df['Age'].clip(0, 100)
df['Gender'] = df['Gender'].map({'F': 0, 'M': 1})
df['No-show'] = df['No-show'].map({'No': 0, 'Yes': 1})

df['ScheduledDay'] = pd.to_datetime(df['ScheduledDay'])
df['AppointmentDay'] = pd.to_datetime(df['AppointmentDay'])
df['DaysInAdvance'] = (df['AppointmentDay'] - df['ScheduledDay']).dt.days.clip(0)

df['AppointmentDayOfWeek'] = df['AppointmentDay'].dt.dayofweek

# Select features
features = ['Gender', 'Age', 'DaysInAdvance', 'AppointmentDayOfWeek',
            'Scholarship', 'Hipertension', 'Diabetes', 'Alcoholism', 'SMS_received']

X = df[features].fillna(0).astype(float)
y = df['No-show']

# Train/test split
X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, random_state=42)

# Train model
model = RandomForestClassifier(n_estimators=100, random_state=42)
model.fit(X_train, y_train)

# Evaluate
y_pred = model.predict(X_test)
print("\nModel Evaluation:")
print(classification_report(y_test, y_pred))

# Export to ONNX
initial_type = [('float_input', FloatTensorType([None, len(features)]))]
onnx_model = convert_sklearn(model, initial_types=initial_type, target_opset=12)

with open('noshow_model.onnx', 'wb') as f:
    f.write(onnx_model.SerializeToString())

print("\nModel exported as noshow_model.onnx")
print("Features used:", features)