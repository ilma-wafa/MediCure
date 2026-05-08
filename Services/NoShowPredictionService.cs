using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace MediCure.Services
{
    public class NoShowPredictionService
    {
        private readonly InferenceSession _session;

        public NoShowPredictionService(IWebHostEnvironment env)
        {
            var modelPath = Path.Combine(env.ContentRootPath, "noshow_model.onnx");
            _session = new InferenceSession(modelPath);
        }

        public NoShowPredictionResult Predict(NoShowInputData input)
        {
            var inputData = new float[]
            {
                input.Gender,
                input.Age,
                input.DaysInAdvance,
                input.AppointmentDayOfWeek,
                input.Scholarship,
                input.Hypertension,
                input.Diabetes,
                input.Alcoholism,
                input.SmsReceived
            };

            var tensor = new DenseTensor<float>(inputData, new[] { 1, 9 });
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", tensor)
            };

            using var results = _session.Run(inputs);
            var resultList = results.ToList();

            var prediction = resultList[0].AsEnumerable<long>().First();

            float noShowProbability;
            try
            {
                var probDict = resultList[1].Value 
                    as IEnumerable<Dictionary<long, float>>;
                    
                if (probDict != null)
                {
                    var first = probDict.First();
                    noShowProbability = first.ContainsKey(1) ? first[1] : 0.3f;
                }
                else
                {
                    noShowProbability = prediction == 1 ? 0.7f : 0.2f;
                }
            }
            catch
            {
                noShowProbability = prediction == 1 ? 0.7f : 0.2f;
            }

            return new NoShowPredictionResult
            {
                WillNoShow = prediction == 1,
                NoShowProbability = Math.Round(noShowProbability * 100, 2),
                RiskLevel = noShowProbability >= 0.5 ? "High" :
                            noShowProbability >= 0.3 ? "Medium" : "Low",
                Recommendation = noShowProbability >= 0.5
                    ? "Send reminder immediately and follow up by phone."
                    : noShowProbability >= 0.3
                    ? "Send SMS reminder 24 hours before appointment."
                    : "Standard reminder sufficient."
            };
        }
    }

    public class NoShowInputData
    {
        public float Gender { get; set; }
        public float Age { get; set; }
        public float DaysInAdvance { get; set; }
        public float AppointmentDayOfWeek { get; set; }
        public float Scholarship { get; set; }
        public float Hypertension { get; set; }
        public float Diabetes { get; set; }
        public float Alcoholism { get; set; }
        public float SmsReceived { get; set; }
    }

    public class NoShowPredictionResult
    {
        public bool WillNoShow { get; set; }
        public double NoShowProbability { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }
}