/***************************************************************************
 *                                                                         *
 *  File:        RouteCalculator.cs                                        *
 *  Copyright:   (c) 2025 Stoian Mario-Daniel                              *
 *  E-mail:      mario-daniel.stoian@student.tuiasi.ro                     *
 *                                                                         *
 *  Description: Această clasă statică, `RouteCalculator`, este proiectată *
 *               pentru a oferi funcționalități de calculare a diverselor  *
 *               metrici asociate unei rute de navigație.                  *
 *                                                                         *
 *               Funcționalitatea sa centrală este expusă prin metoda      *
 *               `CalculateRouteMetrics`. Această metodă primește ca       *
 *               input următoarele date:                                   *
 *                 - Informații despre rută: distanța și durata,           *
 *                   furnizate de obicei de un API extern de rutare.       *
 *                 - Viteza medie de deplasare: specificată de utilizator. *
 *                 - Rata de consum de combustibil: specificată pentru     *
 *                   vehicul.                                              *
 *                                                                         *
 *               Pe baza acestor informații, `CalculateRouteMetrics`       *
 *               efectuează următoarele calcule și returnează:             *
 *                 - Distanța totală a rutei, convertită în kilometri.     *
 *                 - Timpul estimat de parcurgere a rutei, calculat pe     *
 *                   baza distanței și a vitezei medii.                    *
 *                 - Durata călătoriei furnizată de API, convertită într-un*
 *                   obiect `TimeSpan` pentru o manipulare mai ușoară.     *
 *                 - Consumul estimat de combustibil pentru întreaga rută. *
 *                                                                         *
 *               Toate aceste rezultate sunt încapsulate și returnate      *
 *               într-un obiect de tip `CalculationResult`, facilitând     *
 *               accesul la datele calculate.                              *
 *                                                                         *
 *               Clasa include, de asemenea, mecanisme de validare         *
 *               pentru parametrii de intrare, cum ar fi viteza medie și   *
 *               rata de consum, asigurând robustețea calculelor.          *
 *                                                                         *
 ***************************************************************************/

using System;
using RoutingServiceDLL;



namespace RouteCalculatorDLL
{
    /// <summary>
    /// Reprezintă rezultatele calculelor metrice pentru o rută.
    /// Conține timpul estimat calculat, durata furnizată de API, consumul estimat de combustibil
    /// și distanța totală în kilometri.
    /// </summary>
    public class CalculationResult
    {
        /// <summary>
        /// Timpul estimat de parcurgere a rutei, calculat pe baza distanței și a vitezei medii.
        /// </summary>
        public TimeSpan CalculatedEstimatedTime { get; set; }

        /// <summary>
        /// Durata călătoriei așa cum este furnizată de serviciul de rutare (API).
        /// </summary>
        public TimeSpan ApiProvidedDuration { get; set; }

        /// <summary>
        /// Consumul estimat de combustibil (în litri) pentru parcurgerea rutei.
        /// </summary>
        public double EstimatedConsumptionLiters { get; set; }

        /// <summary>
        /// Distanța totală a rutei, exprimată în kilometri.
        /// </summary>
        public double DistanceKm { get; set; }
    }

    /// <summary>
    /// Clasă statică ce oferă metode pentru calcularea metricilor unei rute.
    /// </summary>
    public static class RouteCalculator
    {
        /// <summary>
        /// Calculează diverse metrici pentru o rută dată, incluzând distanța în km, timpul estimat de călătorie
        /// (bazat pe viteza medie), durata furnizată de API și consumul estimat de combustibil.
        /// </summary>
        public static CalculationResult CalculateRouteMetrics(
            RouteInfo routeData,
            double averageSpeedKmH,
            double fuelConsumptionRateL100Km)
        {
            if (routeData == null)
                throw new ArgumentNullException(nameof(routeData), "Datele despre rută (routeData) nu pot fi null.");
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