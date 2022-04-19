# 高级部分

[toc]

## Depth Test

```c++
glEnable(GL_DEPTH_TEST); // 开启深度测试
glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT); // 每帧清空一下对应的buffer

```

<img src="images/depth_test/scene.jpeg" alt="scene" style="zoom:100%;" />

## Depth test function

`glDepthMask(GL_FALSE);`  perform 深度测试但是不更新  depth buffer

`glDepthFunc(GL_LESS);` 默认`GL_LESS`，作为更新 depth buffer 的条件，如果设置为`GL_ALWAYS`每次都会覆盖depth buffer：

<img src="images/depth_test/glDepthFunc(GL_ALWAYS).jpeg" alt="draw with GL_ALWAYS" style="zoom:100%;" />

## Visualizing the depth buffer

可以通过 GLSL 的 built-in 变量 gl_FragCoord 将深度可视化：

<img src="images/depth_test/depth_test(linear depth).jpeg" alt="depth_test" style="zoom:100%;" />

> 涉及到z的精度问题，透视投影将z轴的变化变成了非线性的，变换后的深度正比于1/z ，所以导致近平面处depth精度很大，远处精度很小 （联想到当时games101提出的关于透视投影后，原frustum中点是更靠近近平面还是远平面的问题，答案是远平面。物理上有两种理解方式，一种是说经过“挤压”后，靠近远平面的部分密度更大，所以原来的等距点越靠近远平面就更密集；另一种简单的理解方式是看一条无限长的铁轨，远平面可以认为是无穷，中点无穷的一半，实际上也在透视的那个交点上，自然是更靠近远平面）

将深度可视化之后若想让灰度随深度线性变化，需要通过一系列变换得到线性的深度

```c++
float ndc = depth * 2.0 - 1.0; // [0,1]映射到[-1,1]
float linearDepth = (2.0 * near * far) / (far + near - ndc * (far - near));	 // 用透视矩阵算出投影前后z的关系，表示出原来的z，就是linearDepth

```

 

## Z-fighting

<img src="images/depth_test/z-fighting.jpeg" alt="z-fighting on floor" style="zoom:100%;" />

进入到箱子内部，可以看到本应重合的箱子底面和 plane 随着视角移动疯狂闪烁，由于浮点数精度不足出现了 z-fighting。

1. 物体之间尽量不要太近（不太现实
2. 将近平面设置的尽可能远，因为越靠近近平面精度越高，但是太远会导致裁剪，需要根据实际调整
3. 大多数深度缓冲区的精度为24bit，现在大多GPU也都支持32位的depth buffer，牺牲性能换取更高的精度有助于避免 z-fighting

