### BitVector分析

1. 源代码

```
/*  1. 置位(set)：将某一位置为1.
    2. 清楚位(clear)，清楚某一位，将其置为0.
    3. 读取位(get)，读取某一位的数据，看结果是1还是0.
    4. 容器所能容纳的位个数（size），相当于返回容器的长度。
    5. 被置位的元素个数（count），返回所有被置为1的位的个数。
*/
public sealed class BitVector
{

	private byte[] bits;
	private int size; //容器的长度
    private int count = - 1;//所有被置为1的位的个数

    /// <summary>Constructs a vector capable of holding <code>n</code> bits. </summary>
    /// 指定的参数n表示有多少个数字，相当于要置多少个位
    /// 由于我们要用byte来保存，所以能保存这么多数字的byte个数为n / 8 + 1
    /// 这种长度用移位的方式来表示则为(size >> 3) + 1,右移3位相当于除以8
    public BitVector(int n)
	{
		size = n;
		bits = new byte[(size >> 3) + 1];
	}

    /// <summary>Sets the value of <code>bit</code> to one. </summary>
    /// 前面bit >> 3相当于bit / 8，而bit & 7则相当于bit % 8
    /// 而8对应的二进制表示形式为1000，那么比它小1的7的二进制形式为0111
    /// 在将bit和7进行与运算的时候，所有大于第3位的高位都被置为0，之保留最低的3位
    /// 最低的3位数字最小是0，最大是7.就相当于对数字8求模的运算效
    public void  Set(int bit)
	{
		bits[bit >> 3] |= (byte) (1 << (bit & 7)); //bit % 8,其实就是 1移位 mod后的值
		count = - 1;
	}

    /// <summary>Sets the value of <code>bit</code> to zero. </summary>
    /// 刚好set方法相反,这里是需要将特定的位置为0
    public void  Clear(int bit)
	{
		bits[bit >> 3] &= (byte) ~ (1 << (bit & 7));
		count = - 1;
	}

    /// <summary>Returns <code>true</code> if <code>bit</code> is one and
    /// <code>false</code> if it is zero.
    /// </summary>
    /// 主要是判断这一位是否被置为1
    /// 我们将这个byte和对应位为1的数字求与运算，如果结果不是0，则表示它被置为1.
    public bool Get(int bit)
	{
		return (bits[bit >> 3] & (1 << (bit & 7))) != 0;
	}

	/// <summary>Returns the number of bits in this vector.  This is also one greater than
	/// the number of the largest valid bit number.
	/// </summary>
	public int Size()
	{
		return size;
	}

    /// <summary>Returns the total number of one bits in this vector.  This is efficiently
    /// computed and cached, so that, if the vector is not changed, no
    /// recomputation is done for repeated calls.
    /// </summary>
    /*
     * 如果要计算里面所有被置为1的位的个数，我们需要遍历每个byte，然后求每个byte里面1的个数。
     * 一种想当然的办法就是每次和数字1移位的数字进行与运算，如果结果为0表示该位没有被置为1，否则表示该位有被置位。
     * 这种办法没问题，不过对于每个字节，都要这么走一轮的话，相当于前面运算量的8倍。如果我们可以优化一下的话，对于大数据来说还是有一定价值的。
     * 下面是另一种高效方法的实现，采用空间换时间的办法：
     * BYTE_COUNTS的数组。里面记录了对应一个数字1的个数,我们在bit[i] && 0xff运算之后得到的是一个8位的数字,
     * 范围从0到255.那么，问题就归结到找到对应数字的二进制表示里1的个数
     */
    public int Count()
	{
		// if the vector has been modified
		if (count == - 1)
		{
			int c = 0;
			int end = bits.Length;
			for (int i = 0; i < end; i++)
				c += BYTE_COUNTS[bits[i] & 0xFF]; // sum bits per byte
			count = c;
		}
		return count;
	}
    //255-> 8bit 中整数对应的1的个数,255  1111 1111,16*16=256个数，范围为[0-255]共256个数
	private static readonly byte[] BYTE_COUNTS = new byte[]{
        0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
        1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
        1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
        2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
        1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
        2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
        2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
        3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
        1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
        2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
        2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
        3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
        2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
        3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
        3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
        4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8
    };


	/// <summary>Writes this vector to the file <code>name</code> in Directory
	/// <code>d</code>, in a format that can be read by the constructor {@link
	/// #BitVector(Directory, String)}.  
	/// </summary>
	public void  Write(Directory d, System.String name)
	{
		OutputStream output = d.CreateFile(name);
		try
		{
			output.WriteInt(Size()); // write size
			output.WriteInt(Count()); // write count
			output.WriteBytes(bits, bits.Length); // write bits
		}
		finally
		{
			output.Close();
		}
	}

	/// <summary>Constructs a bit vector from the file <code>name</code> in Directory
	/// <code>d</code>, as written by the {@link #write} method.
	/// </summary>
	public BitVector(Directory d, System.String name)
	{
		InputStream input = d.OpenFile(name);
		try
		{
			size = input.ReadInt(); // read size
			count = input.ReadInt(); // read count
			bits = new byte[(size >> 3) + 1]; // allocate bits
			input.ReadBytes(bits, 0, bits.Length); // read bits
		}
		finally
		{
			input.Close();
		}
	}
}
```

2. 使用场景

2.1 数据位置与统计.通常是考虑到海量的数据的情况下,使用普通的数组会超出数据保存的范围
