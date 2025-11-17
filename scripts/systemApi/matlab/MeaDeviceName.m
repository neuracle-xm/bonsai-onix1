classdef MeaDeviceName
    % 头盒设备名称枚举
    properties
        name
    end

    enumeration
        HubA    ('HubA')
        HubB    ('HubB')
        HubC    ('HubC')
        HubD    ('HubD')
        HubE    ('HubE')
        HubF    ('HubF')
        HubG    ('HubG')
        HubH    ('HubH')
    end

    methods
        function obj = MeaDeviceName(name)
            obj.name = name;
        end
    end
end
