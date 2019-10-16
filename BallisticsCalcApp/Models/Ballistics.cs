﻿using System;

namespace BallisticsCalcApp.Models
{
    public class Ballistics
    {
        // speed of the projectile in meters/second
        public string Velocity { get; set; }
        public string Mass { get; set; }
        public string Diameter { get; set; }
        public string Distance { get; set; }
        public string TempFarenheit { get; set; }
        public string FinalVelocity { get; set; }
        public string BulletDrop { get; set; }
        public string ImpactTime { get; set; }
        public string EstImpactTime { get; set; }
        // TODO not really implemented yet.
        public string AirDensity { get; set; }
        public string DragCoef { get; set; }

        // Double Props
        public double DoubleMeterVelocity { get; set; }
        public double DoubleMassKilos { get; set; }
        public double DoubleAreaMeters { get; set; }
        public double DoubleTargetDistMeters { get; set; }
        public double DoubleDragCoef { get; set; }
        public double pAirDensity = 1.225;
        public double TempCelcius { get; set; }

        // Wind
        public string WindDirection { get; set; }
        public string WindVelocityMPH { get; set; }
        // unused thus far.
        //public double VelocityZ { get; set; }
        public double DistanceZ { get; set; }

        // default constructor
        public Ballistics() { }

        // This method should build out the model for the conroller class.
        public void SetBallistics(string velocity, string mass, string diameter, string distance, string tempFarenheit, string dragCoef)
        {
            // set velocity in meters
            FromFeetPerSecond(velocity);
            // get mass to kg from grains
            ToKilograms(mass);
            // convert caliber to projectile profile area.
            ToArea(diameter);
            // convert yard dist to meter dist.
            ConvertDistance(distance);
            // convert temp to C
            SetTemp(tempFarenheit);

        }


        //  Ballisitics WIND
        public void EstimateWind(string windDirection, string windVelocity)
        {
            int direction = Convert.ToInt32(windDirection);
            double velocity = Convert.ToDouble(windVelocity);
            // convert wind in MPH to meters/second 0.44704
            velocity = velocity * 0.44704;

            double windValue =0;

            if (direction > 330 && direction <= 360 || direction > 0 && direction < 30
                || direction > 150 && direction < 210)
            {
                windValue = 0;
            }
            else if ((direction > 30 && direction < 60) || (direction < 330 && direction > 300)
                || (direction > 210 && direction < 240) || (direction < 150 && direction > 120))
            {
                windValue = 0.75;
            }
            else if ((direction > 60 && direction < 120) ||(direction > 240 && direction < 300))
            {
                windValue = 1.0;
            }
            else
            {
                velocity = 0;
                windValue = 0;
            }

            // Rate * Time = Dist
            // will be in meters...
            DistanceZ = Convert.ToDouble(this.EstImpactTime) * velocity * windValue;
            
        }

        public void DoBallisticsMath()
        {
            Ballistics b = new Ballistics();
            // Call calculate pressure and pass the present temp to configure air density.
            CalculatePressure(TempCelcius);

            const double Gravity = -9.8;

            bool closeDistance = true;
            // double area = Math.Pow(.5 * b.Diameter, 2) * Math.PI;
            int timeCounter = 0;
            double VelocityY = 0, VelocityX = 0;
            // TODO AngleOfBarrel
            // Need to make this an input parameter
            double AngleOfBarrel = 0;
            double p = 1.225; // air density at sea level
            double ForceOfDragX = 0, ForceOfDragY = 0;
            // Not input parameter
            // double coefficientDrag = .410; // that's a good bullet!
            double DistanceX = 0, DistanceY = 0;
            EstImpactTime = Convert.ToString( DoubleTargetDistMeters / DoubleMeterVelocity);

            // initial values
            // Console.WriteLine($"Area of the projectile is {area}, initial velocity is {b.Velocity}");
            // Vy = Vy0 + a*t/2
            VelocityY = DoubleMeterVelocity * Math.Sin(AngleOfBarrel);
            // Vx = Vx0 + a (drag is negative accelleration in this case. factored later)
            VelocityX = DoubleMeterVelocity * Math.Cos(AngleOfBarrel);

            // Console.WriteLine($"Initial values of Velocity of X {b.VelocityX} and Velocity of Y{b.VelocityY}");

            while (closeDistance)
            {
                // 100th of second 

                ///FORCE X & Y
                /// force of drag could be better and more accurately calcultated in Vx and Vy individually
                ForceOfDragX = .5 * p * VelocityX * VelocityX * DoubleDragCoef * DoubleAreaMeters;
                // b.Velocity = b.Velocity - (b.ForceOfDrag / 100);
                ForceOfDragY = .5 * p * VelocityY * VelocityY * DoubleDragCoef * DoubleAreaMeters;

                /// VELOCITY X & Y per 1/100 sec
                // Vx = Vx0 - Df (drag is negative accelleration in this case. factored later)
                VelocityX = VelocityX - (ForceOfDragX * 0.001);
                // gravity is a (-) negative const.
                // Vy = Vy0*t - g*t/2
                VelocityY = VelocityY + 0.5 * Gravity * 0.001;

                /// DISTANCE X & Y
                ///  x = V0x*t + 1/*2ax*t^2 acceleration is Df
                DistanceX = DistanceX + VelocityX * 0.001;


                DistanceY = DistanceY + (VelocityY * 0.001 + 0.5 * Gravity * 0.001 * 0.001);

                // b.TargetDistance = b.TargetDistance - b.DistanceX;

                timeCounter++;

                Console.WriteLine();

                if (DoubleTargetDistMeters < DistanceX)
                {
                    // divide time counter by 100 since each count was 1/100th of a sec
                    ImpactTime = Convert.ToString(timeCounter * 0.001);      
                   // Console.WriteLine($"time counter is {timeCounter}");
                    // Console.WriteLine($"Target Destroyed, time to target was {time} seconds");
                    closeDistance = false;
                }

            }
            // Convert back to Feet per second
            FinalVelocity = Convert.ToString(VelocityX);
            // convert back to feet or inches???
            // need to implement the MOA, MIL drop now.
            BulletDrop = Convert.ToString(DistanceY);

        }

        public double ConvertDistance(string distance)
        {
            // Cast string to double. Then convert from yards to meters
            DoubleTargetDistMeters = Convert.ToDouble(distance);
            DoubleTargetDistMeters *= 1.093613;
            return DoubleTargetDistMeters;
        }
        public string CalculatePressure(double tempCelcius)
        {
            // using defaul of 1ATM
            double pAtmospheres = 1.225;
            pAirDensity = 353.03 * (pAtmospheres / tempCelcius);
            AirDensity = pAirDensity.ToString();
            return AirDensity;
        }


        /// Meters per second from feet per second.
        /// 
        public double FromFeetPerSecond(string velocity)
        {
            DoubleMeterVelocity = Convert.ToDouble(velocity) * 0.3048;
            return DoubleMeterVelocity;
        }

        /// Farenheight to Celcius
        /// 
        public double SetTemp(string tempFarenheit)
        {
            double t = (Convert.ToDouble(tempFarenheit));
            TempCelcius =  (t- 32) * (5 / 9);
            return TempCelcius;
        }

        /// Grains to kilograms
        /// 
        public double ToKilograms(string mass)
        {
            DoubleMassKilos = Convert.ToDouble(mass) * .0000648;
            return DoubleMassKilos;
        }

        /// Area of projectile from diameter
        /// 
        public double ToArea(string diameter)
        {
            double d = Convert.ToDouble(diameter);
            // Convert inch diameter to meter diameter
            d = d * 0.0254;
            // pass in projectile diameter, return area
            double radius = d / 2;
            DoubleAreaMeters = Math.PI * Math.Pow(radius, 2);
            return DoubleAreaMeters;
        }
    }
}
 