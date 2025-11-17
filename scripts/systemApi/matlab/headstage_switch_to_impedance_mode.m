disp('start')
u = udp(Constants.HOST,Constants.PORT);
fopen(u);
switchToImpedanceMode(u, HeadstageDeviceName.HeadstageA, 0);
fclose(u);
disp('end')