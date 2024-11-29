
; General
G90 ; absolute coordinates
M83 ; relative extruder moves
M550 P"Effani" ; set hostname

; Network
M552 S0
M587 S"blah - 2.4Ghz" P"B4forever!"
M552 S1 ; configure WiFi adapter
M586 P0 S1 ; configure HTTP
M586 P1 S1 ; configure FTP
M586 P2 S1 ; configure Telnet

; Wait a moment for the CAN expansion boards to become available
G4 S4

; Smart Drivers
M569 P0.0 S0 D2 ; TableX
M569 P0.1 S0 D2 ; TableY
M569 P1.3 S0 D2 ; Dna1
M569 P1.4 S0 D2 ; Dna2
M569 P2.0 S0 D2 ; Koh2
M569 P2.1 S0 D2 ; Koh1
M569 P2.2 S0 D2 ; Nadh2
M569 P2.3 S0 D2 ; Nadh1
M569 P2.4 S0 D2 ; Basal2
M569 P3.0 S0 D2 ; Basal1


; Motor Idle Current Reduction
M906 I30 ; set motor current idle factor
M84 S30 ; set motor current idle timeout

; Axes
M584 X0.0 Y0.1 D1.3 C1.4 'a2.0 'b2.1 'c2.2 'd2.3 'e2.4 'f3.0 ; set axis mapping
M350 X16 Y16 D16 C16 'a16 'b16 'c16 'd16 'e16 'f16 I1 ; configure microstepping with interpolation
M906 X1700 Y1700 D1500 C1500 'a1500 'b1500 'c1500 'd1500 'e1500 'f1500 ; set axis driver currents
M92 X400 Y400 D1259.84 C1259.84 'a1259.84 'b1259.84 'c1259.84 'd1259.84 'e1259.84 'f1259.84 ; configure steps per mm
M208 X0:312 Y0:364 D0:150 C0:150 'a0:150 'b0:150 'c0:150 'd0:150 'e0:150 'f0:150 ; set minimum and maximum axis limits
M566 X900 Y900 D900 C900 'a900 'b900 'c900 'd900 'e900 'f900 ; set maximum instantaneous speed changes (mm/min)
M203 X5000 Y5000 D2286 C2286 'a2286 'b2286 'c2286 'd2286 'e2286 'f2286 ; set maximum speeds (mm/min)
M201 X500 Y500 D500 C500 'a500 'b500 'c500 'd500 'e500 'f500 ; set accelerations (mm/s^2)


; Kinematics
M669 K0 ; configure Cartesian kinematics

; Endstops
M574 X1 P"!io0.in" S1
M574 Y1 P"!io1.in" S1
M574 D1 P"1.io3.in" S1
M574 C1 P"1.io4.in" S1
M574 'a1 P"2.io0.in" S1
M574 'b1 P"2.io1.in" S1
M574 'c1 P"2.io2.in" S1
M574 'd1 P"2.io3.in" S1
M574 'e1 P"2.io4.in" S1
M574 'f1 P"3.io0.in" S1

; LEDs
M950 P0 C"out3"
