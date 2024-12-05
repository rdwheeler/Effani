
; General
G90 ; absolute coordinates
M83 ; relative extruder moves
M550 P"Effani" ; set hostname

; Network
M552 S1 ; configure WiFi adapter
M586 P0 S1 ; configure HTTP
M586 P1 S1 ; configure FTP
M586 P2 S1 ; configure Telnet

; Wait a moment for the CAN expansion boards to become available
G4 S4

; Smart Drivers
M569 P0.0 S1 D2 ; TableX
M569 P0.1 S0 D2 ; TableY
M569 P0.2 S0 D2 ; Biopsy
M569 P0.3 S1 D2 ; BiopsyEject
M569 P0.4 S1 D2 ; AspirateFluid
M569 P1.0 S1 D2 ; AspirateFluidZ
M569 P1.1 S1 D2 ; AspirateKoh
M569 P1.2 S1 D2 ; AspirateKohZ
M569 P1.3 S0 D2 ; DnaReagent1
M569 P1.4 S0 D2 ; DnaReagent2
M569 P2.0 S0 D2 ; Koh2
M569 P2.1 S0 D2 ; Koh1
M569 P2.2 S0 D2 ; Nadh2
M569 P2.3 S0 D2 ; Nadh1
M569 P2.4 S0 D2 ; Basal2
M569 P1.5 S0 D2 ; Basal1


; Motor Idle Current Reduction
M906 I30 ; set motor current idle factor
M84 S30 ; set motor current idle timeout

; Axes
M584 X0.0 Y0.1 Z0.2 U0.3 V0.4 W1.0 A1.1 B1.2 D1.3 C1.4 'a2.0 'b2.1 'c2.2 'd2.3 'e2.4 'f1.5 ; set axis mapping
M350 X16 Y16 Z16 U16 V16 W16 A16 B16 D16 C16 'a16 'b16 'c16 'd16 'e16 'f16 I1 ; configure microstepping with interpolation
M906 X1700 Y1700 Z1500 U800 V800 W800 A800 B800 D1500 C1500 'a1500 'b1500 'c1500 'd1500 'e1500 'f1500 ; set axis driver currents
M92 X3200 Y3200 Z533.33 U533.33 V2133.33 W533.33 A2133.33 B533.33 D1259.84 C1259.84 'a1259.84 'b1259.84 'c1259.84 'd1259.84 'e1259.84 'f1259.84 ; configure steps per mm
M208 X0:1000 Y0:1000 Z0:200 U0:43 V0:150 W0:43 A0:150 B0:43 D0:150 C0:150 'a0:150 'b0:150 'c0:150 'd0:150 'e0:150 'f0:150 ; set minimum and maximum axis limits
M566 X900 Y900 Z900 U900 V900 W900 A900 B900 D900 C900 'a900 'b900 'c900 'd900 'e900 'f900 ; set maximum instantaneous speed changes (mm/min)
M203 X600 Y600 Z2280 U2280 V2280 W2280 A2280 B2280 D2286 C2286 'a2286 'b2286 'c2286 'd2286 'e2286 'f2286 ; set maximum speeds (mm/min)
M201 X500 Y500 Z500 U500 V500 W500 A500 B500 D500 C500 'a500 'b500 'c500 'd500 'e500 'f500 ; set accelerations (mm/s^2)


; Kinematics
M669 K0 ; configure Cartesian kinematics

; Endstops
M574 X1 P"!io0.in" S1
M574 Y1 P"!io1.in" S1
M574 Z1 P"!io2.in" S1
M574 U1 P"!io3.in" S1
M574 V1 P"!io4.in" S1
M574 W1 P"!1.io0.in" S1
M574 A1 P"!1.io1.in" S1
M574 B1 P"!1.io2.in" S1
M574 D1 P"1.io3.in" S1
M574 C1 P"1.io4.in" S1
M574 'a1 P"2.io0.in" S1
M574 'b1 P"2.io1.in" S1
M574 'c1 P"2.io2.in" S1
M574 'd1 P"2.io3.in" S1
M574 'e1 P"2.io4.in" S1
M574 'f1 P"1.io5.in" S1

; Actuators
M950 P0 C"out1" ; plate UV
M950 P1 C"out3" ; needle UV
M950 P2 C"out6" ; NADH laser
M950 P3 C"2.out6" ; DNA laser
M950 P4 C"out5" ; CO2

; Sensors
M308 S0 P"1.io6.in" Y"linear-analog" A"DNAsensor" F0 B0 C1 ; DNA optical sensor
M308 S1 P"2.io6.in" Y"linear-analog" A"NADHsensor" F0 B0 C1 ; NADH optical sensor
M308 S3 P"2.temp0" Y"thermistor" A"Tray Heater" T100000 B4725 C7.06e-8 ; configure tray heat sensor

; Heaters
M950 H0 C"1.out0" T3 ; create tray heater
M143 H0 P0 T0 C0 S100 A0 ; configure heater monitor
M307 H0 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater
M950 H1 C"1.out1" T3 ; create tray heater
M143 H1 P0 T1 C0 S100 A0 ; configure heater monitor
M307 H1 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater
M950 H2 C"2.out0" T3 ; create tray heater
M143 H2 P0 T2 C0 S100 A0 ; configure heater monitor
M307 H2 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater
M950 H3 C"2.out1" T3 ; create tray heater
M143 H3 P0 T3 C0 S100 A0 ; configure heater monitor
M307 H3 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater

; Heated beds
M140 P0 H0 ; configure bed heater to use the heater^
M140 P1 H1 ; configure bed heater to use the heater^
M140 P2 H2 ; configure bed heater to use the heater^
M140 P3 H3 ; configure bed heater to use the heater^
