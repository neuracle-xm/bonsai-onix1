import ctypes
import numpy as np
from scipy.signal import butter, lfilter, find_peaks
from scipy.linalg import svd
from scipy.spatial import distance_matrix
from sklearn.cluster import KMeans


# print("Success")
# 返回给bonsai的结果
result = 1
# print(f"init result:{result}")
# 通道数
channel_number = 64
# 选择哪一个通道进行Sorting
selected_channel = 0
# 一次传来的时间点
sample_number = 3200
# 采样率
sample_frequency = 64000
# 带通滤波下限截止频率
low_cutoff = 300
# 带通滤波上限截止频率
high_cutoff = 6000
# 巴特沃斯阶数
butter_order = 2
# 从Spike Index向前和向后截取多少数据点
spike_before_samples = 128
spike_after_samples = 128
# 最大类的个数
max_cluster_num = 10
# 先缓存一些Spike Waveform
least_cache_spike_waveform_number = 250
cache_spike_waveforms = []
# 缓存够了之后训练一次，得到一些固定的量，之后来的数据都只进行分类
is_training = True
# 主成分个数
component_number = 10
# PCA的特征向量
u = None
# K-means的聚类中心
k_means_cluster_centers = None
# 空结果
empty_data = np.zeros((max_cluster_num, sample_number), dtype=np.float32)

def set_channel(selected_channel_index):
    """选择哪一个通道进行sorting

    Args:
        selected_channel (_type_): _description_
    """
    global selected_channel
    if selected_channel_index >= channel_number or selected_channel_index < 0:
        selected_channel = 0
    else:
        selected_channel = selected_channel_index

def run(mat):
    global result, is_training, u, k_means_cluster_centers
    # print("Start")
    # print(f"selected_channel:{selected_channel}")
    raw_data = mat2np(mat)
    referenced_data = common_median_reference(raw_data)
    selected_data = referenced_data[selected_channel, :]
    filtered_data = bandpass_filter(
        selected_data,
        low=low_cutoff,
        high=high_cutoff,
        sf=sample_frequency,
        order=butter_order,
    )
    # print(f"filtered_data:{filtered_data[0,:]}")
    # plot_data([raw_data, filtered_data, referenced_data], [0, 1])
    spike_threshold = compute_spike_threshold(filtered_data)
    # print(f"spike_threshold:{spike_threshold[selected_channel]}")
    spike_indices, spike_waveforms = spike_detection(filtered_data, spike_threshold)
    # print(f"spike number:{len(spike_indices[selected_channel])}")
    # 没检测到Spike就直接返回空结果
    if spike_indices.shape[0] == 0:
        # print("no spike detected")
        result = construct_result(empty_data)
        return
    # 至少缓存一定数量的Spike
    if len(cache_spike_waveforms) < least_cache_spike_waveform_number:
        cache_spike_waveforms.extend(spike_waveforms)
        print(f"cache_spike_waveforms:{len(cache_spike_waveforms)}")
        result = construct_result(empty_data)
        return
    # print(f"cache OK:{len(cache_spike_waveforms)}")
    # for i, spike_index in enumerate(spike_indices):
    #     print(f"before spike_indices {i}: {spike_index.shape}")
    # remove_duplicate_spike(spike_indices, spike_waveforms)
    # for i, spike_index in enumerate(spike_indices):
    #     print(f"after spike_indices {i}: {spike_index.shape}")
    if is_training:
        print("training")
        # wavelets = generate_simulated_spike_waveforms()
        # simulated_spike_waveforms = []
        # for _ in range(100):
        #     for wavelet in wavelets:
        #         noise = np.random.normal(0, 0.01, wavelet.shape[0])
        #         simulated_spike_waveforms.append(wavelet + noise)
        # print(f"cache_spike_waveforms:{cache_spike_waveforms}")
        pca_data, u = pca(cache_spike_waveforms)
        print(f"pca_data:{pca_data.shape},u:{u.shape}")
        # /100是防止数据太大
        # pca_data = pca_data / 100
        # print(f"pca_data:{pca_data[0]}")
        # cluster_nodes, nodes = gem_sort(pca_data)
        kmeans = KMeans(n_clusters=max_cluster_num)
        kmeans.fit(pca_data)
        # 保留聚类中心
        k_means_cluster_centers = kmeans.cluster_centers_
        # K-means模板
        # all_templates = [np.zeros((0, cache_spike_waveforms[0].shape[0]))] * len(
        #     kmeans.cluster_centers_
        # )
        # for label in kmeans.labels_:
        #     all_templates[label] = np.append(
        #         all_templates[label],
        #         cache_spike_waveforms[label].reshape(1, -1),
        #         axis=0,
        #     )
        # plt.figure()
        # plt.title("聚类模板")
        # for index, template in enumerate(all_templates):
        #     print(f"模板{index}个数:{template.shape[0]}")
        #     x = template.mean(axis=0)
        #     print(f"x:{x.shape}")
        #     plt.plot(x)
        # plt.show()
        # print(f"cluster_nodes:{cluster_nodes},nodes:{nodes}")
        # compute_template(cluster_nodes, nodes, pca_data, cache_spike_waveforms)
        is_training = False
        print("train done")
        return
    # 之后来的数据都直接分类
    # raster_data = classify_spike(
    #     cluster_nodes,
    #     nodes,
    #     spike_indices[selected_channel],
    #     spike_waveforms[selected_channel],
    #     u,
    #     sample_number,
    # )
    raster_data = k_means_classify_spike(
        spike_indices,
        spike_waveforms,
    )
    # print(f"raster_data:{raster_data}")
    result = construct_result(raster_data)
    # print(f"result:{result}")
    # print("Done")


def mat2np(mat) -> np.ndarray:
    """
    把C#的Mat类型转成numpy数组
    Args:
        mat (_type_): _description_

    Returns:
        np.ndarray: _description_
    """
    # print(f"type:{type(mat)}")
    rows = mat.Rows
    cols = mat.Cols
    # print(f"rows:{rows}, cols:{cols}")
    data_ptr = int(mat.Data.ToInt64())
    buf_size = ctypes.POINTER(ctypes.c_float * (rows * cols))
    buf = ctypes.cast(data_ptr, buf_size).contents
    raw_np_array = np.frombuffer(buf, dtype=np.float32)
    # print(raw_np_array)
    reshape_np_array = raw_np_array.reshape((rows, cols))
    # print(f"shape:{reshape_np_array.shape}")
    # 传过来的数据在C#那边是转置过的，所以要再转置回去
    # transposed_np_array = reshape_np_array.T
    # print(transposed_np_array)
    # print(f"shape:{transposed_np_array.shape}")
    return reshape_np_array


def bandpass_filter(data_raw, low, high, sf, order) -> np.ndarray:
    """
    带通滤波
    Args:
        data_raw (_type_): _description_
        low (_type_): _description_
        high (_type_): _description_
        sf (_type_): _description_
        order (_type_): _description_

    Returns:
        _type_: _description_
    """
    nyq_freq = sf / 2
    low = low / nyq_freq
    high = high / nyq_freq
    b, a = butter(order, [low, high], btype="band")
    # plot_freqz(b, a)
    filtered_data = lfilter(b, a, data_raw)
    # filtered_data = filtfilt(b, a, data_raw, axis=1)
    return filtered_data


def common_median_reference(data: np.ndarray) -> np.ndarray:
    """
    重参考方式:减去所有通道的中位数
    Args:
        data (_type_): _description_

    Returns:
        np.ndarray: _description_
    """
    median_data = np.median(data, axis=0)
    # print(f"median_data:{median_data}")
    return data - median_data


def compute_spike_threshold(data: np.ndarray) -> np.ndarray:
    """
    计算每个通道用于spike detection的阈值
    方式为:5倍的中位数绝对偏差(MAD)转化为标准差
    Args:
        data (np.ndarray): _description_

    Returns:
        np.ndarray: _description_
    """
    sigma = np.median(np.abs(data)) / 0.6745
    spike_detection_threshold = 4 * sigma
    return spike_detection_threshold


def spike_detection(
    data: np.ndarray, spike_detection_threshold: np.ndarray
) -> list[np.ndarray, np.ndarray]:
    """
    Args:
        data (np.ndarray): _description_
        spike_detection_threshold (np.ndarray): _description_

    Returns:
        np.ndarray: _description_
    """
    # 每个通道检测到的Spike Index
    spike_indices = None
    # 从数据中截取出来的Spike Waveform
    spike_waveforms = None
    peaks, _ = find_peaks(np.abs(data), height=spike_detection_threshold, distance=150)
    # print(f"channel {i+1}:{len(p)} peaks detected")
    # 没识别出Spike
    if len(peaks) == 0:
        spike_indices = np.zeros(0)
        spike_waveforms = np.zeros(0)
    else:
        spike_indices = peaks
        # spike_waveforms[i]的形状为 (Spike个数,Spike Waveform的点数)
        spike_waveforms = np.zeros(
            (len(peaks), spike_before_samples + spike_after_samples + 1)
        )
        for j, peak in enumerate(peaks):
            spike_waveforms[j, :] = cut_spike_waveform(data, peak)
    return spike_indices, spike_waveforms


def cut_spike_waveform(data: np.ndarray, spike_index: int) -> np.ndarray:
    """
    从数据中截取Spike Waveform
    Args:
        data (_type_): 形状为(通道数,时间点)
        spike_index (_type_): 这个Spike在数据的第几个位置
    """
    real_before = max(0, spike_index - spike_before_samples)
    real_after = min(spike_index + spike_after_samples + 1, data.shape[0])
    spike_waveform = np.zeros(spike_before_samples + spike_after_samples + 1)
    # 在首尾的Spike长度不够时就在后面补0
    cut_sample = data[real_before:real_after].shape[0]
    spike_waveform[:cut_sample] = data[real_before:real_after]
    return spike_waveform


def pca(spike_waveforms: list[np.ndarray]) -> tuple[np.ndarray, np.ndarray]:
    """
    对某个通道的所有Spike Waveform进行PCA降维
    Args:
        spike_waveforms (_type_): _description_
    """
    # spike_waveform的形状为 (spike个数,时间点)
    spike_waveform = np.array(spike_waveforms)
    # print(f"spike_waveform:{spike_waveform.shape}")
    spike_waveform_transposed = spike_waveform.T
    # 求每个特征的均值
    spike_mean = np.mean(spike_waveform_transposed, axis=1, keepdims=True)
    # 去均值
    spike_demean = spike_waveform_transposed - spike_mean
    # SVD分解
    u, _, _ = svd(spike_demean)
    # print(f"u:{u},s:{s}")
    # 只保留2个主成分
    # 把原始的数据(spike个数,时间点)投影到PC空间(spike个数,主成分个数)
    u = u[:, :component_number]
    pca_data = np.dot(spike_waveform, u)
    return pca_data, u


def k_means_classify_spike(spike_indices: np.ndarray, spike_waveforms: np.ndarray):
    raster_data = np.zeros((max_cluster_num, sample_number), dtype=np.float32)
    pca_data = np.dot(spike_waveforms, u)
    # 计算这些数据和各个中心之间的距离，把他们分到距离最近的那个中心所属的类中
    all_distance = distance_matrix(pca_data, k_means_cluster_centers)
    cluster_indices = all_distance.argmin(axis=1)
    for index, cluster_index in enumerate(cluster_indices):
        spike_index = spike_indices[index]
        raster_data[cluster_index, spike_index] = -150
    return raster_data


def construct_result(raster_data: np.ndarray):
    """_summary_

    Args:
        raster_data (np.ndarray): _description_
    """
    r = {}
    r["rows"] = max_cluster_num
    r["cols"] = sample_number
    r["data"] = raster_data
    r["address"] = raster_data.__array_interface__["data"][0]
    return r


if __name__ == "__main__":
    run(None)
