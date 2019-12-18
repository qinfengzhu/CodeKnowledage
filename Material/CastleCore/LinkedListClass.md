### LinkedList,LinkNode,LinkedListEnumerator 分三部分

1. `LinkNode` 具体结构

```
//LinkedList 链表的Node节点结构
internal class LinkNode
{
    private object _value;
    private LinkNode _next;
    private LinkNode _prev;

    public LinkNode(object value):this(value,null,null){}
    public LinkNode(object value,LinkNode next,LinkNode prev)
    {
        _value = value;
        _next = next;
        _prev = prev;
    }
    public LinkNode next{
        get { return _next; }
        set { _next = value;}
    }
    public LinkNode Previous{
        get { return _prev; }
        set { _prev = value; }
    }
    public object value{
        get{ return _value; }
        set{_value = value; }
    }
}
```

2. `LinkedListEnumerator` 链表的迭代器

```
//链表的迭代器
internal class LinkedListEnumerator : IEnumerator
{
    private LinkNode internalhead;
    private LinkNode _current;
    private bool _isFirst;

    public LinkedListEnumerator(LinkNode node)
    {
        internalhead = node;
        Reset();
    }
    //重置初始状态
    public void Reset()
    {
        _current = internalhead;
        _isFirst = true;
    }
    //当前节点值
    public object Current
    {
        get { return _current.Value; }
    }
    //移动到下一个节点
    public bool MoveNext()
    {
        if(_current==null) return false;
        if(!_isFirst)
        {
            _current = _current.Next;
        }else{
            _isFirst = false;
        }
        return _current != null;
    }
}
```

3. `LinkedList` 具体实现

```
public class LinkedList:IList
{
    private LinkNode internalhead; //头部
    private LinkNode internaltail; //尾部
    private int internalcout;      //链接点总数

    public LinkedList(){}

    //头部节点的值
    public object Head{
        get{
            if(internalhead==null) return null;
            return internalhead.Value;
        }
    }
    //头部添加节点
    public virtual void AddFirst(object value)
    {
        if(internalhead ==null){
            internalhead = new LinkNode(value);
        }else{
            internalhead = new LinkNode(value,internalhead,null);
        }
        internalcout++;
    }
    //尾部添加节点
    public virtual int AddLast(object value)
    {
        if(internalhead==null){
            internalhead = new LinkNode(value);
        }else{
            LinkNode p,q;
            for(p=internalhead;(q=p.Next)!=null;p=q); //简单的循环到尾部去
            p.Next = new LinkNode(value,null,p);
        }
        return internalcout++;
    }
    //添加一个节点
    public virtual int Add(object value)
    {
        if(internalhead==null){
            internalhead = new LinkNode(value);
            internaltail = internalhead;
        }else{
            if(internaltail == null){
                internaltail = internalhead;
                while(internaltail.Next !=null)
                    internaltail = internaltail.Next;
            }
            internaltail.Next = new LinkNode(value,null,internaltail);
            internaltail = internaltail.Next;
        }
        return internalcout++;
    }
    //是否包含
    public bool Contains(object value)
    {
        if(value == null) throw new ArgumentNullException("value");

        foreach(object item in this){
            if(value.Equals(item)){
                return true;
            }
        }
        return false;
    }
    //清空
    public void Clear()
    {
        internalhead = internaltail = null;
        internalcout = 0;
    }
    //节点中的值替换
    public virtual bool Replace(object old,object value)
    {
        LinkNode node = internalhead;
        while(node!=null)
        {
            if(node.Value.Equals(old))
            {
                node.Value = value;
                return true;
            }
            node = node.Next;
        }
        return false;
    }
    //节点的索引位置
    public int IndexOf(object value)
    {
        if(value == null) throw new ArgumentNullException("value");
        int index = -1;
        foreach(object item in this)
        {
            index++;
            if(value.Equals(item))
            {
                return index;
            }
        }
        return -1;
    }
    //插入一个节点到指定的位置
    public virtual void Insert(int index,object value)
    {
        if(index == 0)
        {
            AddFirst(value);
        }
        else if(index == internalcout)
        {
            AddLast(value);
        }else{
            LinkNode insert = GetNode(index);
            LinkNode node = new LinkNode(value,insert,insert.Previous);
            insert.Previous.Next = node;
            insert.Previous = node;
            internalcout++;
        }
    }
    //获取索引的节点
    private LinkNode GetNode(int index)
    {
        ValidateIndex(index);
        LinkNode node = internalhead;
        for(int i = 0;i<index;i++){
            node = node.Next;
        }
        return node;
    }
    //删除节点
    public virtual void Remove(object value)
	{
		if (internalhead != null)
		{
			if (internalhead.Value.Equals(value))
			{
				if (internalhead == internaltail) internaltail = null;

				internalhead = internalhead.Next;

				internalcount--;
			}
			else if (internaltail.Value.Equals(value))
			{
				internaltail.Previous.Next = null;
				internaltail = internaltail.Previous;

				internalcount--;
			}
			else
			{
				LinkNode node = internalhead.Next;

				while(node != null)
				{
					if (node.Value.Equals(value))
					{
						node.Previous.Next = node.Next;
						node.Next.Previous = node.Previous;
						internalcount--;
						break;
					}

					node = node.Next;
				}
			}
		}
	}
    //删除指定位置的节点
	public virtual void RemoveAt(int index)
	{
		throw new NotImplementedException();
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public bool IsFixedSize
	{
		get { return false; }
	}

	public object this[int index]
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	public int Count
	{
		get { return internalcount; }
	}
    //转换为数组
	public Array ToArray(Type type)
	{
		Array array = Array.CreateInstance(type, Count);

		int index = 0;

		foreach(Object value in this)
		{
			array.SetValue(value, index++);
		}

		return array;
	}
    //获取迭代器
    public IEnumerator GetEnumerator()
	{
		return new LinkedListEnumerator(internalhead);
	}
    //
	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}
    //异步Root
	public object SyncRoot
	{
		get { return this; }
	}
    //是否支持多线程
	public bool IsSynchronized
	{
		get { return false; }
	}
}
```
