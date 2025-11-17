disp('start')
u = udp(Constants.HOST,Constants.PORT);
fopen(u);
switchToDataMode(u, HeadstageDeviceName.HeadstageA);
fclose(u);
disp('end')