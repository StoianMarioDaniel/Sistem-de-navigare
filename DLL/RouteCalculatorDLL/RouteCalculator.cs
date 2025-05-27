using System;
using RoutingServiceDLL;

/**************************************************************************
 *                                                                        *
 *  File:        RouteCalculator.cs                                       *
 *  Copyright:   (c) 2025 Stoian Mario-Daniel                             *
 *  E-mail:      mario-daniel.stoian@student.tuiasi.ro                    *
 *                                                                        *
 *  Această clasă statică, `RouteCalculator`, oferă funcționalități       *
 *  pentru calcularea diverselor metrici asociate unei rute de            *
 *  navigație. Principala sa metodă, `CalculateRouteMetrics`, primește    *
 *  informații despre rută (distanță și durată furnizate de un API),      *
 *  o viteză medie de deplasare și o rată de consum de combustibil.       *
 *  Pe baza acestor date, metoda calculează și returnează:                *
 *    - Distanța totală a rutei în kilometri.                             *
 *    - Timpul estimat de parcurgere, calculat pe baza distanței și       *
 *      a vitezei medii.                                                  *
 *    - Durata călătoriei furnizată de API (convertită în TimeSpan).      *
 *    - Consumul estimat de combustibil pentru întreaga rută.             *
 *  Rezultatele sunt încapsulate într-un obiect de tip `CalculationResult`*
 *  Clasa include, de asemenea, validări de bază pentru viteza medie și   *
 *  rata de consum introduse.                                             *
 *                                                                        *
 **************************************************************************/


namespace RouteCalculatorDLL
{
    public class CalculationResult
    {
        public TimeSpan CalculatedEstimatedTime { get; set; }
        public TimeSpan ApiProvidedDuration { get; set; }
        public double EstimatedConsumptionLiters { get; set; }
        public double DistanceKm { get; set; }
    }

    public static class RouteCalculator
    {
        public static CalculationResult CalculateRouteMetrics(
            RouteInfo routeData,
            double averageSpeedKmH,
            double fuelConsumptionRateL100Km)
        {
            if (routeData == null)
                throw new ArgumentNullException(nameof(routeData));
            if (averageSpeedKmH <= 0)
                throw new ArgumentOutOfRangeException(nameof(averageSpeedKmH), "Viteza medie trebuie să fie un număr pozitiv.");
            if (fuelConsumptionRateL100Km < 0)
                throw new ArgumentOutOfRangeException(nameof(fuelConsumptionRateL100Km), "Rata de consum nu poate fi negativă.");

            double distanceKm = routeData.Distance / 1000.0;

            TimeSpan calculatedTime = TimeSpan.FromHours(distanceKm / averageSpeedKmH);

            TimeSpan apiDuration = TimeSpan.FromSeconds(routeData.Duration);

            double consumptionLiters = (distanceKm / 100.0) * fuelConsumptionRateL100Km;

            return new CalculationResult
            {
                DistanceKm = distanceKm,
                CalculatedEstimatedTime = calculatedTime,
                ApiProvidedDuration = apiDuration,
                EstimatedConsumptionLiters = consumptionLiters
            };
        }
    }
}