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
            else if (ButtonType == 1)  //���
            {
                cl.ToDismantle();
            }
            else if (ButtonType == 2)//��λ
            {
                cl.ToReposition();
            }
            else if (ButtonType == (int)MessageType.zhengti_zidongfenbuanzhuang)//�ֲ���װ
            {
                cl.StartIEnumToAssemble();
            }
            else if (ButtonType == (int)MessageType.video_xianquan)//��Ȧ
            {
                cl.Btn_VideoShow1();
            }
            else if (ButtonType == (int)MessageType.video_tiexin)//��о
            {
                cl.Btn_VideoShow2();
            }
            else if (ButtonType == (int)MessageType.video_taogan)//�׹�
            {
                cl.Btn_VideoShow3();
            }
            else if (ButtonType == (int)MessageType.taoguan_btn_fenbuzuzhuang)//�׹ֲܷ���װ
            {
                cl.Btn_TaoGuanZhuang();
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuzuzhuang_s)//�׹ֲܷ���װ����һ��
            {
                cl.IEnum_BtnTaoGuanZhuangSX(0);
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuzuzhuang_x)//�׹ֲܷ���װ����һ��
            {
                cl.IEnum_BtnTaoGuanZhuangSX(1);
            }
            else if (ButtonType == (int)MessageType.taoguan_btn_fenbuchaijie)//�׹ֲܷ����
            {
                cl.Btn_TaoGuanChai();
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuchaijie_s)//�׹ֲܷ�������һ��
            {
                cl.IEnum_BtnTaoGuanChaiSX(0);
            }
            else if (ButtonType == (int)MessageType.taoguan_fenbuchaijie_x)//�׹ֲܷ�������һ��
            {
                cl.IEnum_BtnTaoGuanChaiSX(1);
            }
            else if (ButtonType == (int)MessageType.zhengti_shoudongfenbuanzhuang)//�����ֶ���װ
            {
                cl.Btn_ZhengtiShouDongZuzhuang();
            }
            else if (ButtonType == (int)MessageType.zhengti_shoudongfenbuanchai)//�����ֶ����
            {
                cl.Btn_ZhengtiShouDongChaijie();
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuzuzhuang_s)//�����ֶ�װ��һ��
            {
                cl.IEnum_BtnZhengTiZhuangSX(0);
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuzuzhuang_x)//�����ֶ�װ��һ��
            {
                cl.IEnum_BtnZhengTiZhuangSX(1);
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuchaijie_s)//�����ֶ�����һ��
            {
                cl.IEnum_BtnZhengTiChaiSX(0);
            }
            else if (ButtonType == (int)MessageType.zhengti_fenbuchaijie_x)//�����ֶ�����һ��
            {
                cl.IEnum_BtnZhengTiChaiSX(1);
            }
            else if (ButtonType == (int)MessageType.zhengti_chaizhuangBack)//�����ֶ�����һ��
            {
                cl.ButtonSXBack();
            }
        }
    }
}
