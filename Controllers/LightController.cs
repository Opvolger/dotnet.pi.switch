﻿using System;
using Microsoft.AspNetCore.Mvc;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace api.rpi.gpio.Controllers
{
    // https://jeremylindsayni.wordpress.com/2017/05/01/controlling-gpio-pins-using-a-net-core-2-webapi-on-a-raspberry-pi-using-windows-10-or-ubuntu/
    [Route("api/[controller]")]
    [ApiController]
    public class LightController : ControllerBase
    {
        private readonly GpioPin _pin;

        public LightController()
        {
            _pin = Pi.Gpio[P1.Gpio17];
            _pin.PinMode = GpioPinDriveMode.Output;
        }

        [HttpGet("[action]")]
        public string On()
        {
            this.SwitchPin(true);
            return "On";
        }

        [HttpGet("[action]")]
        public string Off()
        {
            this.SwitchPin(false);
            return "Off";
        }

        private void SwitchPin(bool isOn)
        {
            _pin.Write(!isOn);
        }
    }
}
