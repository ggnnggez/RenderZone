# OctTree Note

### **问题引入**
现有一个巨大的场景，场景内有数量巨大的GameObject，且每个GameObject的Mesh各不相同。需求是付出能够接受的性能，让我们知道每一帧场景内有哪些GameObject相交。
|![问题附图](https://uploads.gamedev.net/monthly_01_2014/ccs-13892-0-48142200-1389658066.png)|
|:--:|
| *Fig(1)* 问题附图 |

### **1.暴力检测**
```c#
foreach(gameObject myObject in ObjList)
{
	foreach(gameObject otherObject in ObjList) 
	{ 
		if(myObject == otherObject) continue; //avoid self collision check 
		if(myObject.CollidesWith(otherObject)) 
		{ 
		//code to handle the collision 
		} 
	} 
}
```
虽然是$O(N^2)$的复杂度，但是在场景中对象较少的时候还是可以用的。

### 2.常规优化思路
在遇到碰撞检测方面的性能瓶颈时可以考虑下面几种方法去优化:

>1. **检查碰撞检测算法内是否包含了大量的`Sqrt()`方法**，由于大部分的原生的开方方法都是使用牛顿迭代法来做，性能消耗较大。特别是计算两个对象距离时，要使用距离的平方而不是平方根来做。

>2. **是否能够减少参与计算碰撞检测的对象？**
   将那些运动确定的且不会与其他对象发生碰撞的对象不做碰撞检测，比如将对象分为静止对象列表与运动对象列表，只对运动对象列表做碰撞检测。

>3. **建立碰撞检测的优先级**

>4. **使用空间划分方法**


### **3.基于空间划分方法的检测**<font size = "2">(二分法 => 四叉树 => 八叉树 随着维度的提高，哪些思想没有发生变化，可以尝试提炼出来，总结成解决对应问题的普适模型 :) )</font>

还是上述的那个场景，我们把场景从中间一分为二，根据对象所处的空间，我们可以把对象放入三个列表中，处于左半边的对象放在列表A中，处于右半边的对象放在列表B中，被划分线穿过的对象我们放在列表C中。
|![空间划分_fig01](https://uploads.gamedev.net/monthly_01_2014/ccs-13892-0-11639700-1389661386.png)|
|:--:|
|*fig(2)* 空间划分示意图|

我们可以确定，ListA中的对象不可能与ListB中的对象相交，故我们只需要对AB列表中的对象各自求交，以及ListC中的对象与A和B中的对象求交。

那么问题来了，如果对象不均匀，比如所有对象全都聚集在空间的左半边，那么这么做空间划分也无济于事。还有对象如果每帧都在移动且趋势无法预测，难道每一帧都要生成一棵树吗？

#### 3.1 **要不要进行分割？怎么进行分割？**
要不要进行分割的准则是:分割是否减少了碰撞检测的次数，而不是为每个对象创造一个完美的包围空间。

是否要继续进行分割的准则:
1.  创造的新的分支若只含有一个对象，那么就不需要再进行分割。(这条规则会被用于八叉树叶子节点的定义)
2.  设置最小分割尺寸。为了防止调用的栈内存溢出，再分隔到最小尺寸的空间后，停止继续分割，最小尺寸内的对象只能用$O(N^2)$的暴力检测方法。
3.  如果区域内不包含任何对象，则不把这块区域放入树中。
如果以上述规则为依据，为二维场景进行划分，结果如下图

| ![四叉树](https://uploads.gamedev.net/monthly_01_2014/ccs-13892-0-51009600-1389735662.png) |
|:--:| 
|*fig(3)* 四叉树示意图|

#### 3.2 **八叉树简介**
所以通过上面二维平面的四叉树推导，我们现在需要把理论推广到三维空间中去
下面是GameDev上对节点类的描述与源码
1. Each node has a bounding region which defines the enclosing region
2. Each node has a reference to the parent node
3. Contains an array of eight child nodes (use arrays for code simplicity and cache performance)
4. Contains a list of objects contained within the current enclosing region
5. I use a byte-sized bitmask for figuring out which child nodes are actively being used (the optimization benefits at the cost of additional complexity is somewhat debatable)
6. I use a few static variables to indicate the state of the tree

```C#
public class OctTree
{ 
	BoundingBox m_region;
	List m_objects; 
	/// 
	/// These are items which we're waiting to insert into the data structure. 
	/// We want to accrue as many objects in here as possible before we inject them into the tree. This is slightly more cache friendly. 
	/// 
	
	static Queue m_pendingInsertion = new Queue(); 
	
	/// 
	/// These are all of the possible child octants for this node in the tree. 
	/// 
	OctTree[] m_childNode = new OctTree[8]; 
	
	///
	/// This is a bitmask indicating which child nodes are actively being used. 
	/// It adds slightly more complexity, but is faster for performance since there is only one comparison instead of 8. 
	///
	byte m_activeNodes = 0;
	
	///
	/// The minumum size for enclosing region is a 1x1x1 cube. 
	///
	const int MIN_SIZE = 1; 
	
	///
	/// this is how many frames we'll wait before deleting an empty tree branch. Note that this is not a constant. The maximum lifespan doubles
	/// every time a node is reused, until it hits a hard coded constant of 64 
	/// 
	int m_maxLifespan = 8; // 
	int m_curLife = -1; //this is a countdown time showing how much time we have left to live 
	
	/// 
	/// A reference to the parent node is nice to have when we're trying to do a tree update. 
	/// 
	OctTree _parent; 
	static bool m_treeReady = false; //the tree has a few objects which need to be inserted before it is complete 
	static bool m_treeBuilt = false; //there is no pre-existing tree yet. 
}
```

#### 3.3 `Init()`
1. 确认整棵树所包含的范围。在初始化整棵树所包围的空间时我们需要做出下面两条设计决策
- 在对象超出所包含的范围时，我们应该怎么做
- 我们应该把包围的空间定义成什么样
  
GamDev上提供的构造函数：
```C#
private OctTree(BoundingBox region, List objList) 
{ 
	m_region = region; 
	m_objects = objList; 
	m_curLife = -1; 
} 

public OctTree() 
{ 
	m_objects = new List(); 
	m_region = new BoundingBox(Vector3.Zero, Vector3.Zero); 
	m_curLife = -1; 
}

public OctTree(BoundingBox region) 
{ 
	m_region = region;
	m_objects = new List(); 
	m_curLife = -1; 
} 
```
由于下文涉及到BoundingBox的应用，所以这里给出《Character Animation With Direct3D》中的对AABB类的定义。下图中蓝色的向量表示两个位于体对角线上的两个点相减，`Vector3 dimensions = m_region.MAX - m_region.MIN`从而得出`dimensions`。

| ![AABB_max-_min](AABB体对角线.png) | 
|:--:| 
| *Fig(4)* AABB的体对角线示意图 |


```C++
class AABB
{
public:
	AABB(D3DXVECTOR3 max, D3DVECTOR3 min)
	{
		m_max = max;
		m_min = min;
	}	

	bool Intersect(D3DVECTOR3 &P)
	{
		if(p.x < m_min.x || p.x > m_max.x)return false;
		if(p.y < m_min.y || p.y > m_max.y)return false;
		if(p.z < m_min.z || p.z > m_max.z)return false;
		return true;
	}

public:
	/*
	* 这里的m_max,m_min 与下文中的m_region.MAX,m_region.MIN对应
	* m_max与m_min分别代表体对角线的两个点
	*/
	D3DVECTOR3 m_max,m_min; 
}
```

1. 要了解[Lazy initialization](https://en.wikipedia.org/wiki/Lazy_initialization#C#)。要尽量拖延内存的分配和构造树，知道我们不得不去做这件的时候。比如果说在用户发出手动插入节点的请求。

#### 3.4 `BuildTree()`
在调用`OctTree(BoundingBox region, List objList)`后能够确定了要进行空间划分的空间和在空间中的对象列表。
开始正式构建OctTree的逻辑步骤
1. 判断当前要生成的节点是否为叶子节点。
```c#
// 上文有说明八叉树叶子节点的定义
if (m_objects.Count <= 1)
	return;
```
2. 获取在初始化时(调用构造函数时)传入的`objList`与`region`，分别获取场景中所包含的对象和场景的大小。
```c#
Vector3 dimensions = m_region.MAX - m_region.MIN;

// 若构造时没有传入region
if (dimensions == Vector3.Zero)
{
	// To create a cube which perfect encloses every single object in the game world.
	FindEnclosingCube(); 

	dimensions = m_region.Max - m_region.Min;
}

// 记住要检查创造出的box是否符合上面规定的MIN_SIZE
if (dimensions.X <= MIN_SIZE && dimensions.Y <= MIN_SIZE && dimensions.Z <=MIN_SIZE)
{
	return;
}
```

3. 分割空间<font size = "1">(下面的代码对划分后空间所对应的BoundingBox进行定义，理解没有什么难度，就不多做介绍。)</font>

```C#
Vector3 half = dimensions/2.0f;
Vector3 center = m_region.Min + half;
BoundingBox[] octant = new BoundingBox[8];

octant[0] = new BoundingBox(m_region.Min,center);
octant[1] = new BoundingBox(new Vector3(center.X, m_region.Min.Y, m_region.Min.Z), new Vector3(m_region.Max.X, center.Y, center.Z));
octant[2] = new BoundingBox(new Vector3(center.X, m_region.Min.Y, center.Z), new Vector3(m_region.Max.X, center.Y, m_region.Max.Z));
octant[3] = new BoundingBox(new Vector3(m_region.Min.X, m_region.Min.Y, center.Z), new Vector3(center.X, center.Y, m_region.Max.Z));
octant[4] = new BoundingBox(new Vector3(m_region.Min.X, center.Y, m_region.Min.Z), new Vector3(center.X, m_region.Max.Y, center.Z));
octant[5] = new BoundingBox(new Vector3(center.X, center.Y, m_region.Min.Z), new Vector3(m_region.Max.X, m_region.Max.Y, center.Z));
octant[6] = new BoundingBox(center, m_region.Max);
octant[7] = new BoundingBox(new Vector3(m_region.Min.X, center.Y, center.Z), new Vector3(center.X, m_region.Max.Y, m_region.Max.Z));
```

4. 分配对象至节点
   分配过程使用到三个容器，分别是`m_object` `octList` `deList`，`m_object`中包含了所有`m_region`中所包含的对象，`octList`中包含了该节点下所包含的对象，`deList`是一个中间容器，记录将要从m_object中剔除的对象。
```C#
// 遍历m_object
foreach (Physical obj in m_objects)
{
	if (obj.BoundingBox.Min != obj.BoundingBox.Max)
	{
		for (int a = 0; a < 8; a++)
		{
			// 若这个对象被对应octant包含，则放入octList和deList中，并开始开始检测下一个m_object中的对象
			if (octant[a].Contains(obj.BoundingBox) == ContainmentType.Contains)
			{
				octList[a].Add(obj);
				delist.Add(obj);
				break;
			}
		}
	}
```

在完成`octList`构建后，从`m_object`中剔除disList中记录的对象
```C#
foreach (Physical obj in delist)
	m_objects.Remove(obj);
```

上述步骤已经阐述清楚树节点的构建逻辑，接下来需要使用相同逻辑进行迭代，继续向下生成树
注意理解`CreateNode()`方法存在的必要性，理解`m_activeNodes`。
```C#
{
	// ......
	// 完成创建父节点

	// 开始迭代创建子节点
	for (int a = 0; a < 8; a++)
	{
		if (octList[a].Count != 0)
		{
			m_childNode[a] = CreateNode(octant[a], octList[a]);
			m_activeNodes |= (byte)(1 << a);
			m_childNode[a].BuildTree();
		}
	}
	
	m_treeBuilt = true;
	m_treeReady = true;
}

// 合法性检测
private OctTree CreateNode(BoundingBox region, List objList) //complete & tested
{
	if (objList.Count == 0)
		return null;
	OctTree ret = new OctTree(region, objList);
	ret._parent = this;
	return ret;
}
```

#### 3.5 `Update()`

##### 实现逻辑
回头看看我们的需求，要求每一帧都需要知道场景内对象的相交情况，故肯定需要我们的树在每一帧对树进行更新。

现在有两个方案：
- 将上一帧的所构建的树丢弃，每一帧重新生成一个新的树
- 以上一帧所构建的树为基础，以这一帧对象的位置为依据，对树的分支进行更新

如果只是数量较少的对象进行了运动，选择去构建整颗树会花费太多的性能去构建一颗大部分分支都重复的树，或许在实际项目中我们需要将**两种方案混合使用**，但在这里我们着重介绍第二种方案。

第二种方案能够使用上述[常规优化思路](#2常规优化思路)中创建动静列表的处理方法，只关注包围盒发生变化的对象，并对那些对象进行操作，下面简述操作步骤。

1. 创建一个列表，记录所有包围盒的位置与大小发生改变的对象。<font size = "2">（依然需要遍历所有节点，这里是否有优化空间?）</font>

2. 每个对象首先判断是否依然存在于当前的节点的包围盒，然后再向上遍历，判断自己处于哪个包围盒。
由于每个节点都储存父级节点的地址，所以能够很轻松的完成向上遍历的操作。

##### 性能优化
上面的步骤涉及对象在树中向上移动，需要注意的是若只考虑向上移动，随着程序运行，越来越多的对象会聚集在树的根节点附近，导致性能急剧下降 <font size = 2>(`m_region`内的对象数量增多，导致计算量剧增)</font> ，所以在实现`Update()`的时候依然要考虑重新构造新的分支，即节点向下移动。这就涉及到**树分支的增加与剔除**，即**内存的申请与释放**。这又是一个很杀性能的操作，在实现中尤其注意这些耗费性能的不起眼操作，因为我们是**逐帧**调用。

对象的运动是不可控的，但又不能频繁申请与释放内存。既然我们需要频繁对这个节点进行释放与申请操作，那我们为什么不在这个节点内没有对象后，在一段时间内依然维持这个节点的存在？那我们该如何得知这个内存的使用频率以及维持多长时间？我们是时候对类`OctTree`进行改造了。
这里选择引入`curLife`与`maxLifespan`来描述节点的寿命。

| 变量名 | 描述 |
| :--- | :-----------: |
| `curLife` | 节点的剩余寿命,即可以这个节点还可以存在多少帧 |
| `maxLifespan` | 节点的最大剩余寿命 |

下面给出实现`Update()`方法的参考代码:

在方法开头，先以一个节点下是否为空为依据，来打开或是关闭剔除倒计时
```C#
public void Update(coreTime time)
{
	if (m_treeBuilt == true && m_treeReady == true)
	{
		// 当叶子节点下面有包含任何对象,开始剔除倒计时
		if (m_object.Count == 0)
		{
			if (HasChildren == false)
			{
				if(m_curLife == -1)
					m_curLife == m_maxLifespan;
				else if (m_curLife > 0)
				{
					m_curLife--;
				}
			}
		}
		else // 用于处理上一帧为空，这一帧又包含了新的对象的情况
		{
			if (m_curLife != -1)
			{
				// double span
				if (m_maxLifespan <= 64)
					m_maxLifespan *= 2;

				// 关闭倒计时
				m_curLife = -1;
			}
		}
	}

	// ......
}

```

在设置好节点的寿命后，申请并更新节点的**运动对象列表**

```C#
{
	// ......
	List<Physical> movedObjects = new List<Physical>(m_objects.Count);

	//go through and update every object in the current tree node
	foreach (Physical gameObj in m_objects)
	{
		//we should figure out if an object actually moved so that we know whether we need to update this node in the tree.
		if (gameObj.Update(time) == 1)
		{
			movedObjects.Add(gameObj);
		}
	}
	//......
}
```

<!-->
> **未完待续**
![2022.6.22](https://www.reactiongifs.us/wp-content/uploads/2013/10/nuh_uh_conan_obrien.gif)
<-->
