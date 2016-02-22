﻿using System;
using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class LowerBathroomConfiguration
    {
        private TimedAction _bathmodeResetTimer;

        public enum LowerBathroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingMiddle,
            LightCeilingWindow,
            // Another light is available!
            CombinedLights,

            StartBathmodeButton,
            LampMirror,

            Window
        }

        public void Setup(Controller controller)
        {
            var hspe16_FloorAndLowerBathroom = controller.Device<HSPE16OutputOnly>(Device.LowerFloorAndLowerBathroomHSPE16);
            var input3 = controller.Device<HSPE16InputOnly>(Device.Input3);
            var i2cHardwareBridge = controller.Device<I2CHardwareBridge>();

            const int SensorPin = 3;

            var bathroom = controller.CreateArea(Room.LowerBathroom)
                .WithMotionDetector(LowerBathroom.MotionDetector, input3.GetInput(15))
                .WithTemperatureSensor(LowerBathroom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(LowerBathroom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(LowerBathroom.LightCeilingDoor, hspe16_FloorAndLowerBathroom.GetOutput(0).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingMiddle, hspe16_FloorAndLowerBathroom.GetOutput(1).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingWindow, hspe16_FloorAndLowerBathroom.GetOutput(2).WithInvertedState())
                .WithLamp(LowerBathroom.LampMirror, hspe16_FloorAndLowerBathroom.GetOutput(4).WithInvertedState())
                .WithWindow(LowerBathroom.Window, w => w.WithCenterCasement(input3.GetInput(13), input3.GetInput(14)));

            bathroom.WithVirtualButton(LowerBathroom.StartBathmodeButton, b => b.WithShortAction(() => StartBathode(bathroom)));

            bathroom.CombineActuators(LowerBathroom.CombinedLights)
                .WithActuator(bathroom.Lamp(LowerBathroom.LightCeilingDoor))
                .WithActuator(bathroom.Lamp(LowerBathroom.LightCeilingMiddle))
                .WithActuator(bathroom.Lamp(LowerBathroom.LightCeilingWindow))
                .WithActuator(bathroom.Lamp(LowerBathroom.LampMirror));

            bathroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(bathroom.MotionDetector(LowerBathroom.MotionDetector))
                .WithTarget(bathroom.BinaryStateOutput(LowerBathroom.CombinedLights));
        }

        private void StartBathode(IArea bathroom)
        {
            bathroom.MotionDetector().Settings.IsEnabled.Value = false;

            bathroom.Lamp(LowerBathroom.LightCeilingDoor).TurnOn();
            bathroom.Lamp(LowerBathroom.LightCeilingMiddle).TurnOff();
            bathroom.Lamp(LowerBathroom.LightCeilingWindow).TurnOff();
            bathroom.Lamp(LowerBathroom.LampMirror).TurnOff();

            _bathmodeResetTimer?.Cancel();
            _bathmodeResetTimer = bathroom.Controller.Timer.In(TimeSpan.FromHours(1)).Do(() => bathroom.MotionDetector().Settings.IsEnabled.Value = true);
        }
    }
}
