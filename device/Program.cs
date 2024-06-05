var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var nr = new NormalRandom();

app.MapGet("/", () =>
{
    var temp = (float)nr.NextDouble()*5+20;
    var hum = (float)nr.NextDouble()*15+30;
    var light = (float)nr.NextDouble()*50+250;
    return new Device(temp, hum, light);
});

app.Run();

public record Device(float Temperature, float Humidity, float Light);


class NormalRandom : Random
{
    // сохранённое предыдущее значение
    double prevSample = double.NaN;
    protected override double Sample()
    {
        // есть предыдущее значение? возвращаем его
        if (!double.IsNaN(prevSample))
        {
            double result = prevSample;
            prevSample = double.NaN;
            return result;
        }

        // нет? вычисляем следующие два
        // Marsaglia polar method из википедии
        double u, v, s;
        do
        {
            u = 2 * base.Sample() -1;
            v = 2 * base.Sample() -1; // [-1, 1)
            s = u * u + v * v;
        }
        while (u <= -1 || v <= -1 || s >= 1 || s == 0);
        double r = Math.Sqrt(-2 * Math.Log(s) / s);

        prevSample = r * v;
        return r * u;
    }
}