using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Ximmerse;
using Ximmerse.XR;
using Ximmerse.XR.UnityNetworking;

public class MessageHandler : MonoBehaviour, TiNetMessageHandler
{

    public static Control cl;

    private void Start()
    {
        cl = GameObject.Find("Canvas").GetComponent<Control>();
    }

    [TiNetMessageCallback(MessageCode.kSynccccccccc)]
    static void OnSyncTransformMessage(TiNetMessage message, I_TiNetNode node)
    {
        syncmessage syncMsg = message as syncmessage;
        int ButtonType = syncMsg.ButtonType;
        if (SyncIdentity.Get(ButtonType, out SyncIdentity entity) && entity != null && entity.IsOwned == false)
        {
            if (ButtonType == 0)  //tuichu 
            {
                cl.Quit();
            }
            else if (ButtonType == 1)  //拆解
            {
                cl.ToDismantle();
            }
            else if (ButtonType == 2)//复位
            {
                cl.ToReposition();
            }
            else if (ButtonType == (int)MessageType.zhengti_zidongfenbuanzhuang)//分步组装
            {
                cl.StartIEnumToAssemble();
            }
            else if (ButtonType == (int)MessageType.video_xianquan)//线圈
            {
                cl.Btn_VideoShow1();
            }
            else if (ButtonType == (int)MessageType.video_tiexin)//铁芯
            {
                cl.Btn_VideoShow2();
            }
            else if (ButtonType == (int)MessageType.video_taogan)//套管
            {
                cl.Btn_VideoShow3();
            }
            else if (ButtonType == (int)MessageType.taoguan_btn_fenbuzuzhuang)//套管分步组装
            {
                cl.Btn_TaoGuanZhuang();
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuzuzhuang_s)//套管分步组装的上一步
            {
                cl.IEnum_BtnTaoGuanZhuangSX(0);
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuzuzhuang_x)//套管分步组装的下一步
            {
                cl.IEnum_BtnTaoGuanZhuangSX(1);
            }
            else if (ButtonType == (int)MessageType.taoguan_btn_fenbuchaijie)//套管分步拆解
            {
                cl.Btn_TaoGuanChai();
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuchaijie_s)//套管分步拆解的上一步
            {
                cl.IEnum_BtnTaoGuanChaiSX(0);
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuchaijie_x)//套管分步拆解的下一步
            {
                cl.IEnum_BtnTaoGuanChaiSX(1);
            }
            else if (ButtonType == (int)MessageType.zhengti_shoudongfenbuanzhuang)//整体手动组装
            {
                cl.Btn_ZhengtiShouDongZuzhuang();
            }
            else if (ButtonType == (int)MessageType.zhengti_shoudongfenbuanchai)//整体手动拆解
            {
                cl.Btn_ZhengtiShouDongChaijie();
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuzuzhuang_s)//整体手动装上一步
            {
                cl.IEnum_BtnZhengTiZhuangSX(0);
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuzuzhuang_x)//整体手动装下一步
            {
                cl.IEnum_BtnZhengTiZhuangSX(1);
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuchaijie_s)//整体手动拆上一步
            {
                cl.IEnum_BtnZhengTiChaiSX(0);
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuchaijie_x)//整体手动拆下一步
            {
                cl.IEnum_BtnZhengTiChaiSX(1);
            }
            else if (ButtonType == (int)MessageType.zhengti_chaizhuangBack)//整体手动拆下一步
            {
                cl.ButtonSXBack();
            }
        }
    }
}
