classdef HeadstageDeviceName
    % Headstage设备名称枚举
    properties
        name
    end

    enumeration
        HeadstageA    ('HeadstageA')
        HeadstageB    ('HeadstageB')
    end

    methods
        function obj = HeadstageDeviceName(name)
            obj.name = name;
        end
    end
end
