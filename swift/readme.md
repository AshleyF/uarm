# uArm Swift Pro

Unlike the [uArm Metal](../metal/readme.md), we've had no desire to replace the firmware on the Swift Pro. Instead, here is a very simple class to communicate with it using the [documented](http://download.ufactory.cc/docs/en/uArm-Swift-Pro-Quick-Start-Guide-1.0.pdf) GCodes.

## Example

    var arm = new UArmSwift("COM9");
    arm.Connect();
    arm.Mode(Mode.UniversalHolder);
    arm.Buzz(10000, 100);
    arm.Buzz(10000, 100);
    Console.WriteLine($"Name: {arm.DeviceName()}");
    Console.WriteLine($"SWVersion: {arm.SoftwareVersion()}");
    Console.WriteLine($"HWVersion: {arm.HardwareVersion()}");
    Console.WriteLine($"APIVersion: {arm.APIVersion()}");
    Console.WriteLine($"Moving: {arm.IsMoving()}");
    arm.MoveXYZ(300, 100, 100, 10000);
    Console.WriteLine($"Moving: {arm.IsMoving()}");
    arm.MoveRTZ(400, 90, 100, 10000);
    arm.Joint(0, 90);
    arm.DetachAll();
    Console.WriteLine($"XYZ: {arm.GetXYZ()}");
    arm.Disconnect();