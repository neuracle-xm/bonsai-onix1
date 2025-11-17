from enum import StrEnum
from pythonosc.udp_client import UDPClient
from pythonosc.osc_message_builder import OscMessageBuilder

# 本机地址
HOST = "127.0.0.1"
# 端口号,和bonsai中的要一致,且不能被其他程序占用
PORT = 50000
# 头盒的最大最小电流,单位μA
MEA_MAX_CURRENT = 4000
MEA_MIN_CURRENT = -4000
# 头盒的通道数
MEA_CHANNEL_NUMBER = 64
# headstage最大最小电流,单位μA
HEADSTAGE_MAX_CURRENT = 2500
HEADSTAGE_MIN_CURRENT = -2500
# headstage的通道数
HEADSTAGE_CHANNEL_NUMBER = 32


class MeaDeviceName(StrEnum):
    """头盒设备名称枚举

    Args:
        StrEnum (_type_): _description_
    """

    HubA = "HubA"
    HubB = "HubB"
    HubC = "HubC"
    HubD = "HubD"
    HubE = "HubE"
    HubF = "HubF"
    HubG = "HubG"
    HubH = "HubH"


class HeadstageDeviceName(StrEnum):
    """Headstage设备名称枚举

    Args:
        StrEnum (_type_): _description_
    """

    HeadstageA = "HeadstageA"
    HeadstageB = "HeadstageB"


def switch_to_data_mode(
    udp_client: UDPClient, device_name: MeaDeviceName | HeadstageDeviceName
) -> None:
    """切换到采集模式

    Args:
        udp_client (UDPClient): UDP客户端
        device_name (MeaDeviceName | HeadstageDeviceName): 头盒或Headstage设备名称
    """
    builder = OscMessageBuilder(address="/")
    builder.add_arg(device_name.value)
    builder.add_arg("data")
    msg = builder.build()
    udp_client.send(msg)


def switch_to_impedance_mode(
    udp_client: UDPClient,
    device_name: MeaDeviceName | HeadstageDeviceName,
    channel_index: int,
) -> None:
    """切换到阻抗模式

    Args:
        udp_client (UDPClient): UDP客户端
        device_name (MeaDeviceName | HeadstageDeviceName): 头盒或Headstage设备名称
        channel_index (int): 需要查看阻抗的那个通道,头盒范围0~63,headstage范围0~31

    Raises:
        ValueError: 阻抗通道参数错误
    """
    if isinstance(device_name, MeaDeviceName) and (
        channel_index < 0 or channel_index >= MEA_CHANNEL_NUMBER
    ):
        raise ValueError(f"头盒阻抗通道channel_index要在0~{MEA_CHANNEL_NUMBER - 1}内")
    if isinstance(device_name, HeadstageDeviceName) and (
        channel_index < 0 or channel_index >= HEADSTAGE_CHANNEL_NUMBER
    ):
        raise ValueError(
            f"Headstage阻抗通道channel_index要在0~{HEADSTAGE_CHANNEL_NUMBER - 1}内"
        )
    builder = OscMessageBuilder(address="/")
    builder.add_arg(device_name.value)
    builder.add_arg("impedance")
    builder.add_arg(channel_index)
    msg = builder.build()
    udp_client.send(msg)


class StimulateParameter:
    """刺激参数类"""

    def __init__(
        self,
        ch_biphasic: bool,
        ch_burst_pulse_count: int,
        ch_enable: bool,
        ch_inter_burst_interval: int,
        ch_inter_phase_current: int,
        ch_inter_phase_interval: int,
        ch_inter_pulse_interval: int,
        ch_phase_one_current: int,
        ch_phase_one_duration: int,
        ch_phase_two_current: int,
        ch_phase_two_duration: int,
        ch_stimulate_channel: int,
        ch_train_burst_count: int,
        ch_trigger_delay: int,
    ) -> None:
        """刺激参数,
        详细见: https://open-ephys.github.io/onix-docs/Hardware%20Guide/Datasheets/estim-hs64.html

        Args:
            ch_biphasic (bool): 是否使用双相波
            ch_burst_pulse_count (int): 每个Burst的脉冲个数
            ch_enable (bool): 是否使用这个通道,这个参数在headstage中无用
            ch_inter_burst_interval (int): 各个Burst之间的间隔,单位μs
            ch_inter_phase_current (int): 双相波的相间电流,单位μA,头盒范围-4000~4000,headstage范围-2500~2500
            ch_inter_phase_interval (int): 双相波的相间脉宽,单位μs
            ch_inter_pulse_interval (int): 每个Burst内部的脉冲间隔,单位μs
            ch_phase_one_current (int): 双相波第一相的电流,单位μA,头盒范围-4000~4000,headstage范围-2500~2500
            ch_phase_one_duration (int): 双相波的第一相脉宽,单位μs
            ch_phase_two_current (int): 双相波的第二相电流,单位μA,头盒范围-4000~4000,headstage范围-2500~2500
            ch_phase_two_duration (int): 双相波的第二相脉宽,单位μs
            ch_stimulate_channel (int): 第几个通道作为刺激通道,头盒范围0~63,这个参数在headstage中无用
            ch_train_burst_count (int): 每个Train的Burst的个数
            ch_trigger_delay (int): 刺激开始前的延迟,单位μs
        """
        self.ch_bi_phasic = ch_biphasic
        self.ch_burst_pulse_count = ch_burst_pulse_count
        self.ch_enable = ch_enable
        self.ch_inter_burst_interval = ch_inter_burst_interval
        self.ch_inter_phase_current = ch_inter_phase_current
        self.ch_inter_phase_interval = ch_inter_phase_interval
        self.ch_inter_pulse_interval = ch_inter_pulse_interval
        self.ch_phase_one_current = ch_phase_one_current
        self.ch_phase_one_duration = ch_phase_one_duration
        self.ch_phase_two_current = ch_phase_two_current
        self.ch_phase_two_duration = ch_phase_two_duration
        self.ch_stimulate_channel = ch_stimulate_channel
        self.ch_train_burst_count = ch_train_burst_count
        self.ch_trigger_delay = ch_trigger_delay

    def check_parameters(self):
        """检验头盒和headstage通用的刺激参数合法性,不合法直接报错"""
        if self.ch_burst_pulse_count <= 0:
            raise ValueError("burst_pulse_count必须大于0")
        if self.ch_inter_burst_interval < 0:
            raise ValueError("ch_inter_burst_interval不能小于0")
        if self.ch_inter_phase_interval < 0:
            raise ValueError("ch_inter_phase_interval不能小于0")
        if self.ch_inter_pulse_interval < 0:
            raise ValueError("ch_inter_pulse_interval不能小于0")
        if self.ch_phase_one_duration < 0:
            raise ValueError("ch_phase_one_duration不能小于0")
        if self.ch_phase_two_duration < 0:
            raise ValueError("ch_phase_two_duration不能小于0")
        if self.ch_train_burst_count < 0:
            raise ValueError("ch_train_burst_count不能小于0")
        if self.ch_trigger_delay < 0:
            raise ValueError("ch_trigger_delay不能小于0")

    def check_mea_stimulate_channel_index(self):
        """检验头盒刺激通道index是否合法"""
        if (
            self.ch_stimulate_channel < 0
            or self.ch_stimulate_channel >= MEA_CHANNEL_NUMBER
        ):
            raise ValueError(
                f"头盒的ch_stimulate_channel必须在0~{MEA_CHANNEL_NUMBER-1}之间"
            )

    @staticmethod
    def check_current_helper(current: int, current_name: str, is_mea: bool):
        """检验头盒和headstage刺激电流是否合法的辅助函数,减少重复

        Args:
            current (int): 电流大小
            current_name (str): 电流属性的字符串
            is_mea (bool): 是不是头盒

        Raises:
            ValueError: _description_
        """
        max_current = MEA_MAX_CURRENT if is_mea else HEADSTAGE_MAX_CURRENT
        min_current = MEA_MIN_CURRENT if is_mea else HEADSTAGE_MIN_CURRENT
        device_name = "头盒" if is_mea else "headstage"
        if current < min_current or current > max_current:
            raise ValueError(
                f"{device_name}的{current_name}必须在{min_current}~{max_current}之间"
            )

    def check_all_current(self, is_mea: bool):
        """检验所有的电流属性

        Args:
            is_mea (bool): 是不是头盒
        """
        StimulateParameter.check_current_helper(
            self.ch_phase_one_current, "ch_phase_one_current", is_mea
        )
        # 如果使用了双相波,再check相间和第二相
        if self.ch_bi_phasic:
            StimulateParameter.check_current_helper(
                self.ch_inter_phase_current, "ch_inter_phase_current", is_mea
            )
            StimulateParameter.check_current_helper(
                self.ch_phase_two_current, "ch_phase_two_current", is_mea
            )

    @staticmethod
    def default_stimulate_parameter() -> "StimulateParameter":
        """使用默认参数构造刺激参数

        Returns:
            StimulateParameter: 默认刺激参数
        """
        return StimulateParameter(False, 1, False, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0)

    @staticmethod
    def check_mea_multi_stimulate_channel_index(
        ch1_stimulate_parameter: "StimulateParameter",
        ch2_stimulate_parameter: "StimulateParameter",
        ch3_stimulate_parameter: "StimulateParameter",
        ch4_stimulate_parameter: "StimulateParameter",
    ):
        """检验头盒四个刺激通道的stimulate_parameter参数是否合法,如果用了这些通道,就不能有相同的

        Args:
            ch1_stimulate_parameter (StimulateParameter): 刺激通道1的参数
            ch2_stimulate_parameter (StimulateParameter): 刺激通道2的参数
            ch3_stimulate_parameter (StimulateParameter): 刺激通道3的参数
            ch4_stimulate_parameter (StimulateParameter): 刺激通道4的参数

        Raises:
            ValueError: _description_
        """
        ch_stimulate_channel1 = ch1_stimulate_parameter.ch_stimulate_channel
        ch_stimulate_channel2 = ch2_stimulate_parameter.ch_stimulate_channel
        ch_stimulate_channel3 = ch3_stimulate_parameter.ch_stimulate_channel
        ch_stimulate_channel4 = ch4_stimulate_parameter.ch_stimulate_channel
        stimulate_channels: list[int] = []
        if ch1_stimulate_parameter.ch_enable:
            stimulate_channels.append(ch_stimulate_channel1)
        if ch2_stimulate_parameter.ch_enable:
            stimulate_channels.append(ch_stimulate_channel2)
        if ch3_stimulate_parameter.ch_enable:
            stimulate_channels.append(ch_stimulate_channel3)
        if ch4_stimulate_parameter.ch_enable:
            stimulate_channels.append(ch_stimulate_channel4)
        if len(stimulate_channels) != len(set(stimulate_channels)):
            raise ValueError("头盒四个刺激通道中的stimulate_channel不能相同")


def add_stimulate_parameter_to_osc_message(
    builder: OscMessageBuilder, stimulate_parameter: StimulateParameter
) -> None:
    """把刺激参数添加到osc的Message中

    Args:
        builder (OscMessageBuilder): 用于创建osc的Message
        stimulate_parameter (StimulateParameter): 被添加的刺激参数
    """
    builder.add_arg(stimulate_parameter.ch_bi_phasic)
    builder.add_arg(stimulate_parameter.ch_burst_pulse_count)
    builder.add_arg(stimulate_parameter.ch_enable)
    builder.add_arg(stimulate_parameter.ch_inter_burst_interval)
    builder.add_arg(stimulate_parameter.ch_inter_phase_current)
    builder.add_arg(stimulate_parameter.ch_inter_phase_interval)
    builder.add_arg(stimulate_parameter.ch_inter_pulse_interval)
    builder.add_arg(stimulate_parameter.ch_phase_one_current)
    builder.add_arg(stimulate_parameter.ch_phase_one_duration)
    builder.add_arg(stimulate_parameter.ch_phase_two_current)
    builder.add_arg(stimulate_parameter.ch_phase_two_duration)
    builder.add_arg(stimulate_parameter.ch_stimulate_channel)
    builder.add_arg(stimulate_parameter.ch_train_burst_count)
    builder.add_arg(stimulate_parameter.ch_trigger_delay)


def mea_stimulate(
    udp_client: UDPClient,
    device_name: MeaDeviceName,
    ch1_stimulate_parameter: StimulateParameter,
    ch2_stimulate_parameter: StimulateParameter,
    ch3_stimulate_parameter: StimulateParameter,
    ch4_stimulate_parameter: StimulateParameter,
) -> None:
    """头盒下发刺激参数

    Args:
        udp_client (UDPClient): UDP客户端
        device_name (DeviceName): 头盒名称
        ch1_stimulate_parameter (StimulateParameter): 刺激通道1的参数
        ch2_stimulate_parameter (StimulateParameter): 刺激通道2的参数
        ch3_stimulate_parameter (StimulateParameter): 刺激通道3的参数
        ch4_stimulate_parameter (StimulateParameter): 刺激通道4的参数
    """
    # 检验所有参数的合法性，如果有不合法会直接报错
    ch1_stimulate_parameter.check_parameters()
    ch1_stimulate_parameter.check_mea_stimulate_channel_index()
    ch1_stimulate_parameter.check_all_current(True)
    ch2_stimulate_parameter.check_parameters()
    ch2_stimulate_parameter.check_mea_stimulate_channel_index()
    ch2_stimulate_parameter.check_all_current(True)
    ch3_stimulate_parameter.check_parameters()
    ch3_stimulate_parameter.check_mea_stimulate_channel_index()
    ch3_stimulate_parameter.check_all_current(True)
    ch4_stimulate_parameter.check_parameters()
    ch4_stimulate_parameter.check_mea_stimulate_channel_index()
    ch4_stimulate_parameter.check_all_current(True)
    StimulateParameter.check_mea_multi_stimulate_channel_index(
        ch1_stimulate_parameter,
        ch2_stimulate_parameter,
        ch3_stimulate_parameter,
        ch4_stimulate_parameter,
    )
    builder = OscMessageBuilder(address="/")
    builder.add_arg(device_name.value)
    builder.add_arg("stimulate")
    add_stimulate_parameter_to_osc_message(builder, ch1_stimulate_parameter)
    add_stimulate_parameter_to_osc_message(builder, ch2_stimulate_parameter)
    add_stimulate_parameter_to_osc_message(builder, ch3_stimulate_parameter)
    add_stimulate_parameter_to_osc_message(builder, ch4_stimulate_parameter)
    msg = builder.build()
    udp_client.send(msg)


def headstage_stimulate(
    udp_client: UDPClient,
    device_name: HeadstageDeviceName,
    ch1_stimulate_parameter: StimulateParameter,
) -> None:
    """Headstage下发刺激参数

    Args:
        udp_client (UDPClient): UDP客户端
        device_name (HeadstageDeviceName): Headstage设备名称
        ch1_stimulate_parameter (StimulateParameter): 刺激通道1的参数
    """
    ch1_stimulate_parameter.check_parameters()
    ch1_stimulate_parameter.check_all_current(False)
    builder = OscMessageBuilder(address="/")
    builder.add_arg(device_name.value)
    builder.add_arg("stimulate")
    add_stimulate_parameter_to_osc_message(builder, ch1_stimulate_parameter)
    msg = builder.build()
    udp_client.send(msg)
