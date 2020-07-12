# serial_console_for_switch
Control your Switch using the keyboard via serial port.

This software is compiled and tested on Windows.

## How to use
1.  Burn the [.hex file](https://github.com/soonuse/stm32_joystick_for_nintendo_switch/releases/download/v1.1/stm32_joystick_for_nintendo_switch.hex) to your STM32F103C development board.
-   See ![stm32_joystick_for_nintendo_switch](https://github.com/soonuse/stm32_joystick_for_nintendo_switch.git) for more information of frame protocol.
2.  Connect the board to your PC with a serial cable.
3.  Connect the board to your Switch using a USB cable. The USB port of switch is type C therefore your might connect Switch to your board with a USB type C converter.
4.  Open this software and open the serial port:
-   Port: COM port for your board
-   Baud: 115200
5.  Then click the button "Open"

![demo](https://github.com/soonuse/serial_console_for_switch/blob/master/SerialConsole/Resources/demo.png)
