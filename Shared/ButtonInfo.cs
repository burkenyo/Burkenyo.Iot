namespace Burkenyo.Iot.Shared;

struct ButtonInfo
#if NETCOREAPP
    : IEquatable<ButtonInfo>
#endif
{
    const byte LeftMask = 0b10000000;
    const byte RightMask = 0b01000000;
    const byte UpMask = 0b00100000;
    const byte DownMask = 0b00010000;
    const byte RedMask = 0b00001000;
    const byte YellowMask = 0b00000100;
    const byte GreenMask = 0b00000010;
    const byte BlueMask = 0b00000001;

    public byte Bits { get; }

    public bool Left =>
        (Bits & LeftMask) != 0;

    public bool Right =>
        (Bits & RightMask) != 0;

    public bool Up =>
        (Bits & UpMask) != 0;

    public bool Down =>
        (Bits & DownMask) != 0;

    public bool Red =>
        (Bits & RedMask) != 0;

    public bool Yellow =>
        (Bits & YellowMask) != 0;

    public bool Green =>
        (Bits & GreenMask) != 0;

    public bool Blue =>
        (Bits & BlueMask) != 0;

    public bool Any =>
        Bits != 0;

    public ButtonInfo(byte bits)
    {
        Bits = bits;
    }

    public ButtonInfo(bool left, bool right, bool up, bool down, bool red, bool yellow, bool green, bool blue)
    {
        Bits = (byte)((left ? LeftMask : 0) | (right ? RightMask : 0) | (up ? UpMask : 0) | (down ? DownMask : 0)
            | (red ? RedMask : 0) | (yellow ? YellowMask : 0) | (green ? GreenMask : 0) | (blue ? BlueMask : 0));
    }

    public void Deconstruct(out bool left, out bool right, out bool up, out bool down, out bool red, out bool yellow, out bool green, out bool blue)
    {
        left = Left;
        right = Right;
        up = Up;
        down = Down;
        red = Red;
        yellow = Yellow;
        green = Green;
        blue = Blue;
    }

    public ButtonInfo NewSince(ButtonInfo other) =>
        new((byte)(Bits & ~other.Bits));

    public override bool Equals(object? obj) =>
        obj is ButtonInfo other && Equals(other);

    public bool Equals(ButtonInfo other) =>
        Bits == other.Bits;

    public override int GetHashCode() =>
        Bits;

    public override string ToString() =>
        $"{nameof(ButtonInfo)} ({nameof(Left)}: {Left}, {nameof(Right)}: {Right}, {nameof(Up)}: {Up}, {nameof(Down)}: {Down}, "
            + $"{nameof(Red)}: {Red}, {nameof(Yellow)}: {Yellow}, {nameof(Green)}: {Green}, {nameof(Blue)}: {Blue})";

    public static bool operator ==(ButtonInfo left, ButtonInfo right) =>
        left.Equals(right);

    public static bool operator !=(ButtonInfo left, ButtonInfo right) =>
        !left.Equals(right);
}
