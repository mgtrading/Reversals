using System;

namespace Reversals
{
    public class BlackScholesCalculator
    {
        public double DOneTime(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                        double dividend)
        {
            //double dOne = Math.Pow(((Math.Log(underlyingPrice / exercisePrice) + (interest - dividend + 0.5 * Math.Pow(volatility, 2))) * time/time) / volatility * 0.512,2);
            double dOne = Math.Pow(((Math.Log(underlyingPrice / exercisePrice) + (interest - dividend * 0.5 * Math.Pow(volatility, 2))) * time / time) / volatility * 0.512, 2);
            return NormSDist(dOne);
        }

        public double DOne(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            
            //double dOne =  (Math.Log(underlyingPrice/exercisePrice) + (interest - dividend + 0.5*Math.Pow(volatility, 2))*time) / (volatility * Math.Sqrt(time));

            double dOne = (Math.Log(underlyingPrice / exercisePrice) + (volatility * volatility * 0.5) * time) / (volatility * Math.Sqrt(time));

            return dOne;
        }

        public double NdOne(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            double ndOne = Math.Exp(-(Math.Pow(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend), 2)) /2) / (Math.Sqrt(2*Math.PI));
            return ndOne;
        //    return Math.Round(ndOne, 2);
        }

        public double DTwo(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            double dTwo = DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend) - volatility*Math.Sqrt(time);
            return dTwo;
        //    return Math.Round(dTwo, 2);
        }

        public double NdTwo(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                         double dividend)
        {
            double ndTwo = DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend);
            return NormSDist(ndTwo);
        }

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
            interest = Math.Log(1 + interest);
            double rho = 1 / (1 + interest) * (-time * Math.Exp(-interest * time) * (exercisePrice * NormSDist(-DTwo(underlyingPrice, exercisePrice, time, interest, volatility, interest)) -
                underlyingPrice * NormSDist(-DOne(underlyingPrice, exercisePrice, time, interest, volatility, interest))));


            //double putRho = -0.01 * exercisePrice * time * Math.Exp(-interest * time) *
            //                 (1 - NormSDist(DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend)));
            return Math.Round(rho / 100,4);
        }
        public double CallRho(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                              double dividend)
        {
            interest = Math.Log(1 + interest);
            double rho = 1/(1+interest)*(-time*Math.Exp(-interest*time)*(underlyingPrice*NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, interest))-
                exercisePrice*NormSDist(DTwo(underlyingPrice, exercisePrice, time, interest, volatility, interest))));
            //return Math.Round(rho / 100,4);

//            double callRho = 0.01 * exercisePrice * time * Math.Exp(-interest * time) * 
//                             NormSDist(DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend));
            return Math.Round(rho / 100,4);
        }

        public double PutTheta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                               double dividend)
        {
            interest = Math.Log(1 + interest);
            double theta =
                -Math.Exp(-interest * time) * underlyingPrice * Norm(DOne(underlyingPrice, exercisePrice, time, interest, volatility, interest)) * volatility / (2 * Math.Sqrt(time))
                - interest * Math.Exp(-interest * time) * (underlyingPrice * NormSDist(-DOne(underlyingPrice, exercisePrice, time, interest, volatility, interest))
                - exercisePrice * NormSDist(-DTwo(underlyingPrice, exercisePrice, time, interest, volatility, interest)));
            
            return Math.Round(theta/365,4);

            //double pt = -(underlyingPrice * volatility * NdOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) /
            //   (2 * Math.Sqrt(time)) + 
            //   interest * exercisePrice * Math.Exp(-interest * (time)) *
            // (1-  NdTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend));

            //double putTheta = pt / 365;

            //return Math.Round(putTheta,4);  
        }
        
        public double Vega(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                           double dividend)
        {
            interest = Math.Log(1 + interest);
            double vega = Math.Exp(-interest * time) * underlyingPrice * NdOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend) * Math.Sqrt(time);
            return Math.Round(vega /100, 4);
        }
        public double Gamma(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                            double dividend)
        {
            interest = Math.Log(1 + interest);
            double gamma = NdOne(underlyingPrice, exercisePrice, time, interest, volatility, interest) /
                           (underlyingPrice * (volatility * Math.Sqrt(time)));
            return Math.Round(gamma, 4);
        }

        public double CallTheta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                double dividend)
        {

            interest = Math.Log(1 + interest);
            double theta =
                -Math.Exp(-interest * time) * underlyingPrice * Norm(DOne(underlyingPrice, exercisePrice, time, interest, volatility, interest)) * volatility / (2 * Math.Sqrt(time))
                + interest * Math.Exp(-interest * time) * (underlyingPrice * NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, interest))
                - exercisePrice * NormSDist(DTwo(underlyingPrice, exercisePrice, time, interest, volatility, interest)));


            //    double ct = 
            //    -(underlyingPrice * volatility * NdOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) /
            //    (2 * Math.Sqrt(time)) -
            //    interest * exercisePrice * Math.Exp(- interest*(time)) *
            //    NdTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend);
            //double callTheta = ct/365;

            //return Math.Round(callTheta,4);
            return Math.Round(theta / 365, 4);

        }

        public double PutDelta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                               double dividend)
        {
            double putDelta  =  NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) - 1;
            return Math.Round(putDelta, 4);
        }

        public double CallDelta(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                double dividend)
        {
            double callDelta = NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend));
            return Math.Round(callDelta, 4);
        }
        public double PutOption(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                double dividend)
        {
            //double putOption = Math.Exp(-interest*time) * (underlyingPrice * NormSDist(-DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) - exercisePrice * NormSDist(-DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend)));
            
            interest = Math.Log(1 + interest);
            double putOption = Math.Exp(-interest * time) * (exercisePrice * NormSDist(-DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) - underlyingPrice * NormSDist(-DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)));
            //double putOption = exercisePrice * Math.Exp(-interest * time) *
            //                   NormSDist(-DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) - Math.Exp(-dividend * time) *
            //                   underlyingPrice * NormSDist(-DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend));
            return Math.Round(putOption, 4);
        }

        public double CallOption(double underlyingPrice, double exercisePrice, double time, double interest, double volatility,
                                 double dividend)
        {
            interest = Math.Log(1 + interest);
            double callOption = Math.Exp(-interest * time) * (underlyingPrice * NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) - exercisePrice * NormSDist(DTwo(underlyingPrice, exercisePrice, time, interest, volatility, dividend)));
//            double callOption = Math.Exp(-dividend * time) * underlyingPrice * NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend)) - exercisePrice * Math.Exp(-interest * time) * NormSDist(DOne(underlyingPrice, exercisePrice, time, interest, volatility, dividend) - volatility * Math.Sqrt(time));

            return Math.Round(callOption, 4);
        }


        public double Norm(double z) //normal probability density function 
        {
            //double a = Math.Exp(-Math.Pow(z, 2) / 2);
            double pi = 3.141592654;
//        double normsdistval = 1 / (Math.Sqrt(2 * Math.PI)) * Math.Exp(-Math.Pow(z, 2) / 2);
        double normsdistval = 1.0 / (Math.Sqrt(2.0 * pi)) * Math.Exp(-(z * z) / 2.0);


        return normsdistval;
        }


        private double NormSDist(double x)
        {
          double GAM = 0.2316419;
          double A1 = 0.319381530;
          double A2 = -0.356563782;
          double A3 = 1.781477937;
          double A4 = -1.821255978;
          double A5 = 1.330274429;
        
          double Nx;
          double Nprimex;
          double k;

          if (x >= 0)
          {
            k = 1.0/(1.0+GAM*x);
            Nprimex = Norm(x);
            Nx= 1-Nprimex*(A1*k+A2*k*k+A3*(k * k * k)+A4*(k * k * k * k)+A5*(k * k * k * k * k));
          }
          else      // Else negative argument
          {
            x = -x;
            k = 1/(1+GAM*x);
            Nprimex = Norm(x);
            Nx = Nprimex*(A1*k+A2*k*k+A3*(k * k * k)+A4*(k * k * k * k)+A5*(k * k * k * k * k));
          }

          return Nx;  // Return cumulative normal distribution approximation value
        }
        
        
        
        
        
//        public double NormSDist(double x) //normal cumulative density function 
//        {
//            const double b0 = 0.2316419;
//            const double b1 = 0.319381530;
//            const double b2 = -0.356563782;
//            const double b3 = 1.781477937;
//            const double b4 = -1.821255978;
//            const double b5 = 1.330274429;
//            double t = 1 / (1 + b0 * x);
//            double a1 = Norm(x);
//            double a2 = (b1 * t + b2 * Math.Pow(t, 2) + b3 * Math.Pow(t, 3) + b4 * Math.Pow(t, 4) + b5 * Math.Pow(t, 5));
//            double mula1A2 = a1*Math.Round(a2,4);
//            double sigma = 1.0 - mula1A2;//(Norm(x) * (b1 * t + b2 * Math.Pow(t, 2) + b3 * Math.Pow(t, 3)
//          //  + b4 * Math.Pow(t, 4) + b5 * Math.Pow(t, 5)));
////james            return sigma;
//            return a2;
//        }

    }
}
