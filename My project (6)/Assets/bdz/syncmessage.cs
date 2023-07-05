using Ximmerse.XR.UnityNetworking;
using UnityEngine;
using TiNetMessgae = Ximmerse.XR.UnityNetworking.TiNetMessage;
using Ximmerse.XR;

[Message(MessageCode.kSynccccccccc)]
public class syncmessage : Ximmerse.XR.UnityNetworking.TiNetMessage
{

    // 1拆解 2组装 3,分步组装   MessageType
    public int ButtonType = 0;


    /// <summary>
    /// 从网络读取数据
    /// </summary>
    public override void OnDeserialize()
    {

        ButtonType = ReadInt();
    }

    /// <summary>
    /// 向网络写出数据
    /// </summary>
    public override void OnSerialize()
    {

        WriteInt(ButtonType);
    }
}

enum MessageType
{
    zhengti_chaijie =1,
    zhengti_fuwei =2,
    zhengti_zidongfenbuanzhuang =3,

    video_xianquan =4,
    video_tiexin = 5,
    video_taogan = 6,

    taoguan_btn_fenbuzuzhuang = 7,
    taoguan_btn_fenbuchaijie =8,

    taoguan_fenbuzuzhuang_s = 9,
    taoguan_fenbuzuzhuang_x = 10,

    taoguan_fenbuchaijie_s = 11,
    taoguan_fenbuchaijie_x = 12,

    zhengti_shoudongfenbuanzhuang = 13,
    zhengti_shoudongfenbuanchai = 14,
    zhengti_fenbuzuzhuang_s = 15,
    zhengti_fenbuzuzhuang_x = 16,
    zhengti_fenbuchaijie_s = 17,
    zhengti_fenbuchaijie_x = 18,

    zhengti_chaizhuangBack = 19,
}
