using System;
using System.Diagnostics;
using System.Drawing;

namespace Brief.Robotics
{
    public class RobotArm
    {
        private readonly double _xShift;
        private readonly double _yShift;
        private double _zShift = 0;
        private readonly double _minDimensionHalf;

        public Point LastPoint { get; set; }

        public double ZShift
        {
            get { return _zShift; }
            set
            {
                _zShift = value;
                Move(LastPoint);
            }
        }

        private readonly IArm _arm;

        public RobotArm(double xShift, double yShift, double minDimensionHalf, IArm arm)
        {
            _xShift = xShift;
            _yShift = yShift;
            _minDimensionHalf = minDimensionHalf;
            _arm = arm;
            Connect();
        }

        public void Connect()
        {
            _arm.Connect();
            _connected = true;
        }

        public void Disconnect()
        {
            if (!Connected) return;

            ArmDown(false); // Lift the arm.

            // Now disconnect the arm.
            _arm.Disconnect();
            _connected = false;
        }

        public void Close()
        {
            Disconnect();
        }

        private bool _connected;
        public bool Connected => _connected;

        private bool _armIsDown;
        public bool ArmIsDown => _armIsDown;

        public void ArmDown(bool down)
        {
            if (!Connected) return;
            _armIsDown = down;
            Move(LastPoint);
        }

        private bool _scaraMode = true;

        public void MoveRT(double r, double t)
        {
            if (!Connected) return;

            var x = r * Math.Sin(t);
            var y = r * Math.Cos(t);
            var z = (ArmIsDown ? 0.0 : 0.4) - ZShift;
            _arm.Move(x, y, z, _scaraMode);
        }

        private const double ScalingFactorX = 1.2;
        private const double ScalingFactorY = 1.0;

        public void Move(Point pt)
        {
            if (!Connected) return;

            LastPoint = pt;
            var scale = 1.0;
            var x = ((pt.Y - _yShift) / _minDimensionHalf * scale * ScalingFactorX);
            var y = ((pt.X - _xShift) / _minDimensionHalf * scale * ScalingFactorY);

            // convert to polar to compute backlash
            var r = Math.Sqrt(x * x + y * y);
            var t = Math.Atan2(x, y); // right-hand coords (x = -y, y = x)

            MoveRT(r, t);
        }

        private const double FactorR = 100.0;
        private const double FactorT = 100.0;

        public void Home()
        {
            if (!Connected) return;

            ArmDown(false);
            Move(new Point(0, 0));
        }
    }
}