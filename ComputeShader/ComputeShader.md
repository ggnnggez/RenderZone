# Compute Shader Note

> 目的:尝试将 ImageViewer工具中的逐像素对比运算加速

### 问题引入
需要渲染40000个点，则CPU需要向GPU传输 $40000$ 个 $4\times 4$ 的变换矩阵，每个矩阵占 $16\times4=64$ 字节。仅仅是变换矩阵的一次提交就占 $40000\times 64$ 字节。如果算上阴影与深度的PASS，则需要提交三次如此之大的数据量，如果对渲染的帧率要求是 $60$ Fps，那么**一秒**CPU则需要向GPU提交 $60\times 3 \times 64\times 40000 = 460,800,000 $(*byte*) 即大约 $440$ *MB*的变换矩阵数据。

### Compute Shader 介绍
<!-->
##### 概念介绍
##### GPU运算的特性
##### 如何与cpu交互
##### 如何与传统渲染管线交互
##### 支持的硬件平台
<-->
首先先介绍*D3D11*中有关[Compute Shader](https://docs.microsoft.com/en-us/windows/win32/direct3d11/direct3d-11-advanced-stages-compute-shader)的一些概念:

| Function Name | Desc |
| :--- | :---|
|  `Dispatch(uint x,uint y, uint y)` | 在`ID3D11DeviceContext`命名空间下，能够通过调用该函数执行 *compute shader* 的 *command list* 中的命令。  |


| Semantics  | Desc |
| :--- | :--- |
| `SV_GroupID` | Thread Group的编号，下图有详细标注，一个Group中包含了一个`numthreads()`所申请的线程 |
| `SV_GroupIndex` |  |
| `numthreads(uint x,uint y, uint y)`| 使用三个维度的方法描述数量与分布，申请**一组**特定数量拥有特定分布的线程 |
| `SV_DispatchThreadID` | 一个线程的全局编号 |
| `SV_GroupThreadID` | 一个线程的组内编号 |

|![@](https://docs.microsoft.com/en-us/windows/win32/api/d3d11/images/threadgroupids.png)|
|:---:|
|[*Fig(1)* ](https://docs.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch) *Thread Group*与*Thread*之间的层次关系图|