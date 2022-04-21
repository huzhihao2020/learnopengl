[toc]

# Depth Test

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

<img src="images/depth_test/depth_test(linear depth).jpeg" alt="depth_test(linear depth)" style="zoom:100%;" />

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

# Stencil Test

stencil buffer 中通常存储 8bit 的数据，可以用于剔除或保留具有某一 stencil 值的 fragments

stencil buffer 的使用与 depth buffer 类似，通过`glEnable(GL_STENCIL_TEST); ` 开启 Stencil Test，同时每帧需要额外 `glClear(GL_STENCIL_BUFFER_BIT); `另外可以通过 `glStencilMask(0xFF);` 或者`glStencilMask(0x00);`来定义写入stencil buffer 的 bit-mask 

```cpp
glEnable(GL_STENCIL_TEST);
glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
glStencilFunc(GL_NOTEQUAL, 1, 0xFF); // stencil value 和 ref 都 masked 之后再比较
glStencilMask(0xFF); // 写入 stencil buffer 时经过的 mask
```

## Stencil Fucntion

共有两个函数可以配置 stencil testing

* `glStencilFunc(GLenum func, GLint ref, GLuint mask)`

  ref表示stencil test 与ref比较

* `glStencilOp(GLenum sfail, GLenum dpfail, GLenum dppass)` 

  * sfail 表示 stencil test fail 了应该怎么办
  * dpfail 是 stencil test 通过但是 depth test 没通过的情况
  * dppass表示都通过的情况

  默认参数为` (GL_KEEP, GL_KEEP, GL_KEEP)`

## Object outlining

可以使用 stencil test 勾勒出物体的轮廓，步骤如下：

1. `glENABLE(GL_STENCIL_TEST)` 启用 STENCIL_TEST
2. 渲染物体之前先把 stencil buffer 用1填满 (GL_ALWAYS)
3. 渲染场景
4. 关闭 stencil test 和 depth test
5. 将要加轮廓的物体scale放大一点点后，用轮廓的颜色重新渲染(只渲染stencil buffer不为1的地方)
6. 将 depth test和stencil test 的状态还原，以免影响后续物体的渲染

下面给 Depth Test 场景中的两个箱子加上轮廓：

<img src="images/stencil_test/stencil_test.jpeg" alt="stencil_test" style="zoom:100%;" />

多个物体边缘重叠：

<img src="images/stencil_test/stencil_test_overlap.jpeg" alt="stencil_test_overlap" style="zoom:100%;" />

为了视觉美观可以将边缘加一个高斯模糊。

# Blending

`glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data);`的format 设为RGBA 可以读带 alpha 通道的图，但是 opengl 默认不处理 alpha 值，像之前那样渲染会得到：

<img src="images/blend/blend_grass_noalpha.jpeg" alt="blend_grass_noalpha" style="zoom:100%;" />

## Discarding Fragments

一般在 fragment shader 里可以将 alpha 小的部分 discard 掉

```cpp
vec4 texColor = texture(texture1, TexCoords);
if(texColor.a < 0.1)
    discard;
FragColor = texColor;
```

现在得到：

<img src="images/blend/blend_grass_alpha.jpeg" alt="blend_grass_alpha.jpeg" style="zoom:100%;" />

> bind 带 alpha 的 texture 一般不用默认的 warp mode，因为 GL_REPEAT 会使上下、左右底边插值，可以在 glBindTexture 之后将 warp mode 改成 GL_CLAMP_TO_EDGE
> `glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE); `
> `glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);`

## Blending

discard 掉一些 fragment 也没有给我们渲染版透明物体的能力，可以用下面的语句开启：

```
glEnable(GL_BLEND);  
```

fragment shader 运行且各种 test 都通过之后，其输出和 framebuffer 中的颜色进行混合，可以通过 ` glBlendFunc(GLenum sfactor, GLenum dfactor)` 调整 src factor 和 dest factor，`glBlendEquation(GLenum mode)` 还可以指定 Src 和 Dst 的加减等关系

## Semi-transparent Textures

开启 blending，应用到半透明的纹理上，效果如下

<img src="images/blend/blend_semi-transparent_window.jpeg" alt="blend_semi-transparent_window" style="zoom:100%;" />

仔细观察，发现透明物体之间遮挡关系不对，原来是深度测试出了问题，深度测试时并不会关心 fragment 是否透明，为了解决这个问题，我们需要更改渲染顺序

1. 先渲染不透明物体
2. 根据透明物体的深度进行排序
3. 透明物体从远到近渲染

使用 std::map 创建物体到相机距离 distance 和其对应 model 变换的映射，根据 distance 排序：

<img src="images/blend/blend_semi-transparent_sorted_window.jpeg" alt="blend_semi-transparent_sorted_window" style="zoom:100%;" />

# Face Culling

## Winding Order & Face Culling

 OpenGL's GL_CULL_FACE option:

```
gl_Enable(GL_CULL_FACE);
glCullFace(GL_FRONT);  
glFrontFace(GL_CCW); // counter-clockwise
```

需要注意 OpenGL 默认 CCW winding，Direct3D 默认 CW winding。

<img src="images/face_culling/face_culling.jpeg" alt="face_culling" style="zoom:100%;" />

# Framebuffers

## Create Framebuffers

创建 & 绑定 FBO in opengl

```cpp
unsigned int fbo;
glGenFramebuffers(1, &fbo);
glBindFramebuffer(GL_FRAMEBUFFER, fbo);
```

这样创建好的 Framebuffer 还不能用，要 attach 到 color, depth 或 stencil buffer 中的一种才可以。而且每个 buffer 都要有相同的 sample 数量

```cpp
glBindFramebuffer(GL_FRAMEBUFFER, 0);   // 希望render输出到屏幕需要绑定到0(默认framebuffer)
```

在上述操作中我们往 fbo 绘制的数据并没有出现在屏幕上，所以叫做 off-screen 渲染，framebuffer 对应的 attachment 实际上就是一块可以存储 framebuffer 数据的内存，可以选择两种 attachment : texture 或者 renderbuffer objects

## Texture attachments

将 framebuffer attach 到 texture 上，前面的过程与加载 texture 类似，只不过 data 变成了 NULL，而且尺寸与屏幕大小相同

```cpp
unsigned int texture;
glGenTexture(1, &texture);
glBindTexture(GL_TEXTURE_2D, texture);

glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, 800, 600, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);

glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);  
```

attach 操作 ：

```cpp
glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texture, 0);
```

参数从前到后解释，`GL_FRAMEBUFFER` 表示 target 的 framebuffer type (read/write) ，`GL_COLOR_ATTACHMENT0` 表示 attachment type  (可以一对多)，`GL_TEXTURE_2D` 表示想要 attach 的 texture 的类型，`texture`是实际的 texture 变量，`0`是 mipmap 的 level。

后面还可以给这个 framebuffer attach depth 或者 stencil texture，一般会分给 depth(24bit） 和 stencil(8bit) 放到一起，使用`GL_DEPTH_STENCIL_ATTACHMENT`

## Renderbuffer object attachments

另一种方式是使用 rbo，使用  rbo  会把所有的 render data 直接存入 buffer中，不会把数据转换成 texture 的格式；但是不能直接从 rbo 中读值，需要通过` glReadPixels`一次取一小片区域的数据。综上 opengl 可以对其采取特殊的存储优化。正是因为 rbo 不能直接读，所以更适合存储 depth 和 stencil 数据，因为一般不需要对这两个值采样，只需要进行深度&模板测试

rbo 的具体使用方法也是类似：

```cpp
// create rbo & bind to framebuffer
unsigned int rbo;
glGenRenderbuffers(1, &rbo);
glBindRenderbuffer(GL_RENDERBUFFER, rbo);  
// specify the rbo as a depth24_stencil8 rbo
glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, 800, 600);
glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, rbo);  
```

## Rendering to a texture

采用 framebuffer + rbo 的方式，往 texture 里面渲染需要下面三步：

1. `glBindTexture(GL_TEXTURE_2D, framebuffer);`
2. Bind to the default framebuffer.
3. Draw a quad that spans the entire screen with the new framebuffer's color buffer as its texture.

此时 Framebuffer 中的图像：

<img src="images/framebuffer/framebuffer_offscreen.jpeg" alt="face_culling" style="zoom:70%;" />

屏幕上的图像 (开启`glPolygonMode(GL_FRONT_AND_BACK, GL_LINE)`)，实际是将 framebuffer 上的图像作为texture 绘制到屏幕上的矩形 

<img src="images/framebuffer/framebuffer_onscreen.jpeg" alt="face_culling" style="zoom:70%;" />

## Post-processing

以 framebuffer 做纹理可以对原图像在 shader 中进行一些操作，这就是后处理，比如：

```cpp
    FragColor = vec4(vec3(1.0 - texture(screen_texture, TexCoords)), 1.0);
```

只需改一行就能得到反相的输出

<img src="images/framebuffer/framebuffer_post-process.jpeg" alt="face_culling" style="zoom:100%;" />

或者其他滤波或者风格化的图像
