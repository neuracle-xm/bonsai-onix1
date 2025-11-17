function switchToDataMode(u,deviceName)
    % 切换到采集模式
    oscsend(u,'/','ss', deviceName.name,'data');
end