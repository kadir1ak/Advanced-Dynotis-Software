using System;

namespace Advanced_Dynotis_Software.Services.Controllers
{
    public class PIDController
    {
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }

        private double _previousError;
        private double _integral;
        private double _integralMax;
        private double _integralMin;
        private double _previousDerivative;
        private double _alpha; // Smoothing factor for derivative

        public PIDController(double kp, double ki, double kd, double integralMax = 50.0, double integralMin = -50.0, double alpha = 0.01)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
            _integralMax = integralMax;
            _integralMin = integralMin;
            _alpha = alpha; // Smoothing factor for derivative
        }

        public double Calculate(double setPoint, double actualValue)
        {
            double error = setPoint - actualValue;
            _integral += error;

            // Limit the integral (anti-windup)
            if (_integral > _integralMax) _integral = _integralMax;
            else if (_integral < _integralMin) _integral = _integralMin;

            double derivative = error - _previousError;
            // Apply smoothing to the derivative term
            double smoothedDerivative = _alpha * derivative + (1 - _alpha) * _previousDerivative;
            _previousDerivative = smoothedDerivative;

            _previousError = error;

            double output = Kp * error + Ki * _integral + Kd * smoothedDerivative;

            return output;
        }

        public void Reset()
        {
            _previousError = 0;
            _integral = 0;
            _previousDerivative = 0;
        }
    }
}