using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroschrauberSharp
{
    public class RemoteInput
    {
        public RemoteInput()
        {
        }

        public float roll  { get; set; }
        public float pitch  { get; set; }
        public float yaw { get; set; }
        public float throttle { get; set; }

        public bool active { get; set; }

        public bool switch1 { get; set; }
        public bool switch2 { get; set; }
    }

    public class SensorInput
    {
        public SensorInput()
        {
        }

        public VectorFloat accel;
        public VectorFloat gyro;
        public VectorFloat magneto;
        public float ultrasound_down = -1f;
    }

    public class MotorOutput
    {
        public MotorOutput()
        {
        }

        public float motor_left;
        public float motor_right;
        public float motor_front;
        public float motor_back;
    }

    /*
     * ctrl_gain_x 0.100000
ctrl_gain_y 0.100000
ctrl_gain_z 0.100000
ctrl_inner_p_x 0.230000
ctrl_inner_p_y 0.230000
ctrl_inner_p_z -0.500000
ctrl_inner_i_x 0.400000
ctrl_inner_i_y 0.400000
ctrl_inner_i_z 0.000000
ctrl_inner_d_x 0.000000
ctrl_inner_d_y 0.000000
ctrl_inner_d_z 0.000000
ctrl_inner_im_x 0.100000
ctrl_inner_im_y 0.100000
ctrl_inner_im_z 0.200000
ctrl_outer_p_x 6.000000
ctrl_outer_p_y 6.000000
ctrl_outer_p_z 0.000000
ctrl_outer_i_x 0.300000
ctrl_outer_i_y 0.300000
ctrl_outer_i_z 0.000000
ctrl_outer_d_x 0.000000
ctrl_outer_d_y 0.000000
ctrl_outer_d_z 0.000000
ctrl_outer_im_x 0.200000
ctrl_outer_im_y 0.200000
ctrl_outer_im_z 0.100000
remote_gain_x 0.400000
remote_gain_y 0.400000
remote_gain_z 2.000000
altitude_p 0.200000
*/

    public class Controller
    {
        public Controller()
        {
            inner.P = new VectorFloat(0.23f, 0.23f, -0.5f);
            inner.I = new VectorFloat(0.4f, 0.4f, 0f);
            inner.D = new VectorFloat(0f, 0f, 0f);
            inner.I_max = new VectorFloat(0.1f, 0.1f, 0.2f);
            outer.P = new VectorFloat(6f, 6f, 0f);
            outer.I = new VectorFloat(0.3f, 0.3f, 0f);
            outer.D = new VectorFloat(0f, 0f, 0f);
            outer.I_max = new VectorFloat(0.2f, 0.2f, 0.1f);
            gain = new VectorFloat(0.1f, 0.1f, 0.1f);
            remote_gain = new VectorFloat(0.4f, 0.4f, 2.0f);
        }

        public void Update(float dtime, RemoteInput remote, SensorInput sensors, MotorOutput output)
        {
            gyro_filter.Filter(sensors.gyro, dtime, 1000, 1000, 4);
            accel_filter.Filter(sensors.accel, dtime, 10);
            magneto_filter.Filter(sensors.magneto, dtime, 2);

            if (accel_filter.Current.y == 0 && accel_filter.Current.z == 0)
                ahrs.x = 0;
            else
                ahrs.x = (ahrs.x + gyro_filter.Current.x * dtime) * 0.98f + (float)Math.Atan2(accel_filter.Current.y, accel_filter.Current.z) * 0.02f;

            if (accel_filter.Current.x == 0 && accel_filter.Current.z == 0)
                ahrs.y = 0;
            else
                ahrs.y = (ahrs.y + gyro_filter.Current.y * dtime) * 0.98f - (float)Math.Atan2(accel_filter.Current.x, accel_filter.Current.z) * 0.02f;

            ahrs.z = ahrs.z + gyro_filter.Current.z * dtime;

            ahrs_dev = gyro_filter.Current;

            //Console.WriteLine(accel_filter.Current.x + " " + accel_filter.Current.y + " " + accel_filter.Current.z);
            //Console.WriteLine("ypr: " + ahrs.y + " " + ahrs.z + " " + ahrs.z);

            VectorFloat setpoint = new VectorFloat(remote.roll * remote_gain.x, remote.pitch * remote_gain.y, remote.yaw * remote_gain.z);

            outer_pid_output = outer.Update(setpoint - ahrs, dtime);
            outer_pid_output.z = setpoint.z;

            inner_pid_output = inner.Update(outer_pid_output - ahrs_dev, dtime);

            inner_pid_output *= gain;
            const float max_control = 0.2f;
            inner_pid_output.x = MathUtil.Clamp(inner_pid_output.x, -max_control, max_control);
            inner_pid_output.y = MathUtil.Clamp(inner_pid_output.y, -max_control, max_control);
            inner_pid_output.z = MathUtil.Clamp(inner_pid_output.z, -max_control, max_control);

            float altitude_compensation = 0.0f;
            /*
            if(altitude_hold < 0 && remote.switch1 && remote.active && sensors.ultrasound_down > 0)
            {
                //altitude hold turned on
                altitude_hold = sensors.ultrasound_down;
            }
            else if(altitude_hold >= 0 && !remote.switch1 && remote.active)
            {
                //altitude hold turned off
                altitude_hold = -1.0f;
            }
    
            if(altitude_hold >= 0 && sensors.ultrasound_down > 0)
            {
                altitude_compensation = (sensors.ultrasound_down - altitude_hold) * 0.2f;
                CLAMP(altitude_compensation, -0.2f, 0.2f);
            }
            else if(fabs(ahrs.x) < RAD(40) && fabs(ahrs.y) < RAD(40))
            {
                altitude_compensation = (1.0f - accel_filter.z) * altitude_P;
            }
            */

            if (remote.active && remote.throttle >= 0.05f)
            {
                output.motor_left = remote.throttle + inner_pid_output.x - inner_pid_output.z + altitude_compensation;
                output.motor_right = remote.throttle - inner_pid_output.x - inner_pid_output.z + altitude_compensation;
                output.motor_front = remote.throttle + inner_pid_output.y + inner_pid_output.z + altitude_compensation;
                output.motor_back = remote.throttle - inner_pid_output.y + inner_pid_output.z + altitude_compensation;
            }
            else
            {
                output.motor_left = 0;
                output.motor_right = 0;
                output.motor_front = 0;
                output.motor_back = 0;
            }

        }

        public VectorLowPass gyro_filter = new VectorLowPass();
        public VectorLowPass accel_filter = new VectorLowPass();
        public VectorLowPass magneto_filter = new VectorLowPass();

        public VectorFloat inner_pid_output;
        public VectorFloat outer_pid_output;

        public VectorFloat ahrs;
        public VectorFloat ahrs_dev;
        public VectorFloat ahrs_int;

        public VectorFloat remote_gain = new VectorFloat(0.50f, 0.50f, 0.50f);
        public VectorFloat gain = new VectorFloat(0.05f, 0.05f, 0.05f);


        public VectorPID inner = new VectorPID();
        public VectorPID outer = new VectorPID();

        public float altitude_P = 0.2f;
        public float altitude_hold = -1;

        protected VectorFloat ahrs_before;
        protected VectorFloat ahrs_dev_before;
        protected bool first = true;
    }
}
