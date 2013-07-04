using System;

namespace Reversals
{
    public class BlackScholesCalculator
    {
        public double DOneTime(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                        double dividend)
        {
            double dOne = Math.Pow(((Math.Log(underlyingPrice / exercisePrice) + (interest - dividend + 0.5 * Math.Pow(volatility, 2))) * time/time) /
                          volatility * 0.512,2);
            return NormSDist(dOne);
        }

        public double DOne(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            
            double dOne =  (Math.Log(underlyingPrice/exercisePrice) + (interest - dividend + 0.5*Math.Pow(volatility, 2))*time) / 
                          (volatility * Math.Sqrt(time));
            return dOne;
        }

        public double NdOne(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            double ndOne = Math.Exp(-(Math.Pow(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend), 2)) /2)
                           /
                           (Math.Sqrt(2*Math.PI));
            return Math.Round(ndOne,4);
        }

        public double DTwo(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            double dTwo = DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend) -
                          volatility*Math.Sqrt(time);
            return Math.Round(dTwo,4);
        }

        public double NdTwo(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            double ndTwo = DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend);
            return NormSDist(ndTwo);
        }


        //___________________________________________
        /*
        Function ImpliedCallVolatility(UnderlyingPrice, ExercisePrice, Time, Interest, Target, Dividend)
High = 5
Low = 0
Do While (High - Low) > 0.0001
If CallOption(UnderlyingPrice, ExercisePrice, Time, Interest, (High + Low) / 2, Dividend) > Target Then
High = (High + Low) / 2
Else: Low = (High + Low) / 2
End If
Loop
ImpliedCallVolatility = (High + Low) / 2
End Function
         */
        //___________________________________________


        public double ImpliedPutVolatility(double underlyingPrice, double exercisePrice, double time, double interest, double target, double dividend)
        {
            double high = 5;
            double low = 0;

            while ((high - low) > 0.0001)
            {
                if (PutOption(underlyingPrice, exercisePrice, time, interest, (high + low) / 2, dividend) > target)
                    high = (high + low) / 2;
                else
                    low = (high + low) / 2;
            }
            double impliedPutColatility = (high + low) / 2;
            return impliedPutColatility;
        }

        public double ImpliedCallVolatility(double underlyingPrice, double exercisePrice, double time, double interest, double target,
                             double dividend)
        {
            double high = 5;
            double low = 0;

           while ((high - low) > 0.0001) 
            {
                if(CallOption(underlyingPrice, exercisePrice, time, interest, (high + low )/2, dividend) > target)
                    high = (high + low)/2;
                else
                    low = (high + low)/2;
            } 
            double impliedCallColatility = (high + low)/2;
            return impliedCallColatility;
        }

        public double PutRho(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                     double dividend)
        {  
            double putRho = -0.01 * exercisePrice * time * Math.Exp(-interest * time) *
                             (1 - NormSDist(DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend)));
            return Math.Round(putRho,4);
        }
        public double CallRho(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                              double dividend)
        {

            double callRho = 0.01 * exercisePrice * time * Math.Exp(-interest * time) * 
                             NormSDist(DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend));
            return Math.Round(callRho,4);
        }
        public double PutTheta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                               double dividend)
        {
   
            double pt = -(underlyingPrice * volatility * NdOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) /
               (2 * Math.Sqrt(time)) + 
               interest * exercisePrice * Math.Exp(-interest * (time)) *
             (1-  NdTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend));

            double putTheta = pt / 365;

            return Math.Round(putTheta,4);  
        }
        public double Vega(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                           double dividend)
        {
            double vega = 0.01*underlyingPrice*Math.Sqrt(time) *
                          NdOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend);
            return Math.Round(vega,4);
        }
        public double Gamma(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                            double dividend)
        {
            double gamma = NdOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend) /
                           (underlyingPrice * (volatility * Math.Sqrt(time)));
            return Math.Round(gamma,4);
        }

        public double CallTheta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                double dividend)
        {
    
            double ct = 
                -(underlyingPrice * volatility * NdOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) /
                (2 * Math.Sqrt(time)) -
                interest * exercisePrice * Math.Exp(- interest*(time)) *
                NdTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend);
            double callTheta = ct/365;

            return Math.Round(callTheta,4);
        }

        public double PutDelta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                               double dividend)
                {
                  double putDelta  =  NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) - 1;
                    return Math.Round(putDelta,4);
                }

        public double CallDelta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                double dividend)
        {
            double callDelta = NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend));
            return Math.Round(callDelta,4);
        }
        public double PutOption(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                double dividend)
        {
   

            double putOption = exercisePrice*Math.Exp(-interest*time)*
                               NormSDist(-DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) -
                               Math.Exp(-dividend*time) * underlyingPrice *
                               NormSDist(-DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend));
            return Math.Round(putOption,4);
        }

        public double CallOption(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                 double dividend)
        {
            double callOption = Math.Exp(-dividend * time) * underlyingPrice 
                                * NormSDist(DOne(underlyingPrice,exercisePrice,time,interest,volatility,dividend)) - exercisePrice * Math.Exp(-interest * time) 
                                * NormSDist(DOne(underlyingPrice,exercisePrice,time,interest,volatility,dividend) - volatility * Math.Sqrt(time));

            return Math.Round(callOption,4);

        }


        public double Norm(double z) //normal probability density function 
        {
            //double a = Math.Exp(-Math.Pow(z, 2) / 2);
    
        double normsdistval = 1 / (Math.Sqrt(2 * Math.PI)) * Math.Exp(-Math.Pow(z, 2) / 2);
        return normsdistval;
        }
        public double NormSDist(double x) //normal cumulative density function 
        {
            const double b0 = 0.2316419;
            const double b1 = 0.319381530;
            const double b2 = -0.356563782;
            const double b3 = 1.781477937;
            const double b4 = -1.821255978;
            const double b5 = 1.330274429;
            double t = 1 / (1 + b0 * x);
            double a1 = Norm(x);
            double a2 = (b1 * t + b2 * Math.Pow(t, 2) + b3 * Math.Pow(t, 3)
            + b4 * Math.Pow(t, 4) + b5 * Math.Pow(t, 5));
            double mula1A2 = a1*Math.Round(a2,4);
            double sigma = 1.0 - mula1A2;//(Norm(x) * (b1 * t + b2 * Math.Pow(t, 2) + b3 * Math.Pow(t, 3)
          //  + b4 * Math.Pow(t, 4) + b5 * Math.Pow(t, 5)));
            return sigma;
        }

    }
}
