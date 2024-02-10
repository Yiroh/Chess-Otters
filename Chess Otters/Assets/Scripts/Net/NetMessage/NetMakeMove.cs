using Unity.Networking.Transport;

public class NetMakeMove : NetMessage
{
    public int originalX;
    public int originalY;
    public int destionationX;
    public int destionationY;
    public int teamId;

    public NetMakeMove()
    {
        Code = OpCode.MAKE_MOVE;
    }
    public NetMakeMove(DataStreamReader reader)
    {
        Code = OpCode.MAKE_MOVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt(originalX);
        writer.WriteInt(originalY);
        writer.WriteInt(destionationX);
        writer.WriteInt(destionationY);
        writer.WriteInt(teamId);
    } 
    public override void Deserialize(DataStreamReader reader) 
    {
        originalX = reader.ReadInt();
        originalY = reader.ReadInt();
        destionationX = reader.ReadInt();
        destionationY = reader.ReadInt();
        teamId = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }
}
