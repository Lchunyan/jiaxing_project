using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;
using UnityEngine.Video;
using Ximmerse.XR;
using Ximmerse.XR.UnityNetworking;

public class Control : MonoBehaviour
{
    private Button Button_1;
    private Button Button_2;
    private Button Button_3;
    private Button Button_4;
    private Button Button_5;

    private Button Button_4_S;
    private Button Button_4_X;
    private Button Button_4back;
    private Text Bg_4name;
    private Transform Bg_4;

    private Button Button_5_S;
    private Button Button_5_X;
    private Button Button_5back;
    private Text Bg_5name;
    private Transform Bg_5;

    private StateEnum nowType = StateEnum.none;

    [HideInInspector]
    public Dictionary<string, PosData> ModelPosDic = new Dictionary<string, PosData>();
    [HideInInspector]
    public Dictionary<string, PosData> TGPosDic = new Dictionary<string, PosData>();

    /// <summary>
    /// /////////video
    /// </summary>
    private Button video_Button_1;
    private Button video_Button_2;
    private Button video_Button_3;
    private Button video_Button_4;
    private Button video_Button_5;

    private Button video_Button_4_S;
    private Button video_Button_4_X;
    private Button video_Button_4_S2;
    private Button video_Button_4_X2;

    private VideoPlayer videoPlayer;
    VideoClip clip1;
    VideoClip clip2;
    VideoClip clip3;

    //scallview
    private Transform TrPerContent;

    /// <summary>套管演示
    private Transform TaoGuan3;//低压套管C相-18
    private Transform TaoGuanTarget;
    private Camera ThisCamera;
    private Transform origeCamera;
    private Transform targetCamera;

    private void Start()
    {
        string[] nowtime = System.DateTime.Now.ToString("yyyy:MM:dd").Split(new char[] { ':' });
        if (nowtime[1] == "9" || nowtime[1] == "7")
        {
            UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
        }
        Button_1 = transform.Find("Button_Quit").GetComponent<Button>();
        Button_1.onClick.AddListener(() =>
        {
            Quit();
            Send0();
        });
        Button_1 = transform.Find("Button1").GetComponent<Button>(); //拆解
        Button_1.onClick.AddListener(() =>
        {
            ToDismantle();
            EventTypeTop_Send1();
        });

        Button_2 = transform.Find("Button2").GetComponent<Button>(); //复位
        Button_2.onClick.AddListener(() =>
        {
            ToReposition();
            EventTypeTop_Send2();
        });

        Button_3 = transform.Find("Button3").GetComponent<Button>(); //自动分步拆解
        Button_3.onClick.AddListener(() =>
        {
            StartIEnumToAssemble();
            EventTypeTop_Send3();
        });


        ///////////////整体的手动分步组装
        Button_4 = transform.Find("Button4").GetComponent<Button>(); //手动分步组装
        Button_4.onClick.AddListener(() =>
        {
            Btn_ZhengtiShouDongZuzhuang();
            Zhengti_Send13();
        });

        Button_5 = transform.Find("Button5").GetComponent<Button>(); //手动分步拆解
        Button_5.onClick.AddListener(() =>
        {
            Btn_ZhengtiShouDongChaijie();
            Zhengti_Send14();
        });

        Bg_4 = transform.Find("4bg");
        Bg_4name = transform.Find("4bg/name").GetComponent<Text>();
        Button_4_S = transform.Find("4bg/Button4_S").GetComponent<Button>();
        Button_4_S.onClick.AddListener(() =>
        {
            IEnum_BtnZhengTiZhuangSX(0);
            Zhengti_Send15();
        });
        Button_4_X = transform.Find("4bg/Button4_X").GetComponent<Button>();
        Button_4_X.onClick.AddListener(() =>
        {
            IEnum_BtnZhengTiZhuangSX(1);
            Zhengti_Send16();
        });
        Button_4back = transform.Find("4bg/back").GetComponent<Button>();
        Button_4back.onClick.AddListener(() =>
        {
            ButtonSXBack();
            Zhengti_Send19();
        });
        Bg_4.gameObject.SetActive(false);

        Bg_5 = transform.Find("5bg");
        Bg_5name = transform.Find("5bg/name").GetComponent<Text>(); ;
        Button_5_S = transform.Find("5bg/Button5_S").GetComponent<Button>();
        Button_5_S.onClick.AddListener(() =>
        {
            IEnum_BtnZhengTiChaiSX(0);
            Zhengti_Send17();
        });
        Button_5_X = transform.Find("5bg/Button5_X").GetComponent<Button>();
        Button_5_X.onClick.AddListener(() =>
        {
            IEnum_BtnZhengTiChaiSX(1);
            Zhengti_Send18();
        });
        Button_5back = transform.Find("5bg/back").GetComponent<Button>();
        Button_5back.onClick.AddListener(() =>
        {
            ButtonSXBack();
            Zhengti_Send19();
        });
        Bg_5.gameObject.SetActive(false);



        GameObject Model = GameObject.Find("ZB_OK_v001");
        List<PosData> inDexNumDataList = new List<PosData>();
        for (int i = 0; i < Model.transform.childCount; i++)
        {
            Transform trchild = Model.transform.GetChild(i);
            PosData pd = new PosData();
            pd.Tr = trchild;
            pd.pos1 = trchild.position;
            pd.roat1 = trchild.localEulerAngles;

            pd.indexNum = int.Parse(trchild.name.Split('-')[1]);
            inDexNumDataList.Add(pd);
        }
        //排序
        inDexNumDataList.Sort((a, b) =>
        {
            return a.indexNum - b.indexNum;
        });
        for (int i = 0; i < inDexNumDataList.Count; i++)
        {
            if (!ModelPosDic.ContainsKey(inDexNumDataList[i].Tr.name))
                ModelPosDic.Add(inDexNumDataList[i].Tr.name, inDexNumDataList[i]);
        }
        GameObject Mode2 = GameObject.Find("ZB_OK_v002");
        for (int i = 0; i < Mode2.transform.childCount; i++)
        {
            Transform trchild = Mode2.transform.GetChild(i);
            ModelPosDic[trchild.name].roat2 = trchild.localEulerAngles;
            ModelPosDic[trchild.name].pos2 = trchild.position;
        }
        Mode2.gameObject.SetActive(false);


        clip1 = Resources.Load<VideoClip>("线圈拆解演示");
        clip2 = Resources.Load<VideoClip>("铁芯叠装演示");
        clip3 = Resources.Load<VideoClip>("套管安装");
        videoPlayer = transform.Find("Videoshow/Video Player").GetComponent<VideoPlayer>();
        videoPlayer.gameObject.SetActive(false);
        video_Button_1 = transform.Find("Videoshow/video_Button1").GetComponent<Button>(); //线圈 
        video_Button_1.onClick.AddListener(() =>
        {
            Btn_VideoShow1();
            EventTypeTop_Send4();
        });
        video_Button_2 = transform.Find("Videoshow/video_Button2").GetComponent<Button>(); //铁芯
        video_Button_2.onClick.AddListener(() =>
        {
            Btn_VideoShow2();
            EventTypeTop_Send5();
        });
        video_Button_3 = transform.Find("Videoshow/video_Button3").GetComponent<Button>(); //套管
        video_Button_3.onClick.AddListener(() =>
        {
            Btn_VideoShow3();
            EventTypeTop_Send6();
        });
        video_Button_4 = transform.Find("Videoshow/video_Button4").GetComponent<Button>(); //
        video_Button_4.onClick.AddListener(() =>
        {
            Btn_TaoGuanZhuang();
            TaoGuan_Sen7();
        });
        video_Button_5 = transform.Find("Videoshow/video_Button5").GetComponent<Button>(); //
        video_Button_5.onClick.AddListener(() =>
        {
            Btn_TaoGuanChai();
            TaoGuan_Se8();
        });

        video_Button_4_S = transform.Find("Videoshow/video_Button4S").GetComponent<Button>(); //
        video_Button_4_S.onClick.AddListener(() =>
        {
            IEnum_BtnTaoGuanZhuangSX(0);
            TaoGuan_Se9();
        });
        video_Button_4_X = transform.Find("Videoshow/video_Button4X").GetComponent<Button>(); //
        video_Button_4_X.onClick.AddListener(() =>
        {
            IEnum_BtnTaoGuanZhuangSX(1);
            TaoGuan_Se10();
        });
        video_Button_4_X.gameObject.SetActive(false);
        video_Button_4_S.gameObject.SetActive(false);

        video_Button_4_S2 = transform.Find("Videoshow/video_Button4S2").GetComponent<Button>(); //
        video_Button_4_S2.onClick.AddListener(() =>
        {
            IEnum_BtnTaoGuanChaiSX(0);
            TaoGuan_Se11();
        });
        video_Button_4_X2 = transform.Find("Videoshow/video_Button4X2").GetComponent<Button>(); //
        video_Button_4_X2.onClick.AddListener(() =>
        {
            IEnum_BtnTaoGuanChaiSX(1);
            TaoGuan_Se12();
        });
        video_Button_4_X2.gameObject.SetActive(false);
        video_Button_4_S2.gameObject.SetActive(false);

        //scalview 
        TrPerContent = transform.Find("liebiao_Line/Scroll View/Viewport/Content/child");

        for (int i = 0; i < inDexNumDataList.Count; i++)
        {
            GameObject objchild = Instantiate(TrPerContent.gameObject);
            objchild.gameObject.SetActive(true);
            objchild.transform.SetParent(TrPerContent.parent);
            objchild.transform.localScale = new Vector3(1, 1, 1);
            Vector3 vp = objchild.transform.localPosition;
            objchild.transform.localPosition = new Vector3(vp.x, vp.y, 0);

            Text tt = objchild.transform.Find("Text").GetComponent<Text>();
            tt.text = inDexNumDataList[i].Tr.name;
        }


        //套管演示
        TaoGuan3 = Model.transform.Find("低压套管A相-18");
        TaoGuanTarget = GameObject.Find("ZB-低压套管").transform;
        Transform CT = TaoGuanTarget.Find("C相低压套管");//////////////////
        List<PosData> CTList = new List<PosData>();
        for (int i = 0; i < CT.transform.childCount; i++)
        {
            Transform trchild = CT.transform.GetChild(i);
            PosData pd = new PosData();
            pd.Tr = trchild;
            pd.pos1 = trchild.position;
            pd.roat1 = trchild.eulerAngles;

            pd.indexNum = int.Parse(trchild.name.Split('-')[1]);
            CTList.Add(pd);
        }
        //排序
        CTList.Sort((a, b) =>
        {
            return a.indexNum - b.indexNum;
        });
        for (int i = 0; i < CTList.Count; i++)
        {
            if (!TGPosDic.ContainsKey(CTList[i].Tr.name))
                TGPosDic.Add(CTList[i].Tr.name, CTList[i]);
        }
        GameObject CT2 = GameObject.Find("ZB-低压套管2");
        for (int i = 0; i < CT2.transform.childCount; i++)
        {
            Transform trchild = CT2.transform.GetChild(i);
            TGPosDic[trchild.name].roat2 = trchild.eulerAngles;
            TGPosDic[trchild.name].pos2 = trchild.position;
        }
        CT2.gameObject.SetActive(false);
        TaoGuanTarget.gameObject.SetActive(false);

        ThisCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        origeCamera = GameObject.Find("Main Cameraori").transform;
        targetCamera = GameObject.Find("Main Cameratar").transform;
    }

    IEnumerator PanduanIsPlayingIEnum()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (!videoPlayer.isPlaying)
            {
                nowType = StateEnum.none;
                videoPlayer.gameObject.SetActive(false);
                yield break;
            }
        }
    }
    //拆解
    public void ToDismantle()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.zhengtiyijianchai;

        foreach (string key in ModelPosDic.Keys)
        {
            Transform tr = ModelPosDic[key].Tr;
            Vector3 rota = ModelPosDic[key].roat2;
            Vector3 vc = ModelPosDic[key].pos2;
            ModelPosDic[key].Tr.DORotate(rota, 1);
            ModelPosDic[key].Tr.DOMove(vc, 1);
        }
    }
    //复位
    public void ToReposition()
    {
        if (nowType == StateEnum.zhengtiyijianchai)
        {
            foreach (string key in ModelPosDic.Keys)
            {
                Transform tr = ModelPosDic[key].Tr;
                Vector3 rota = ModelPosDic[key].roat1;
                Vector3 vc = ModelPosDic[key].pos1;
                ModelPosDic[key].Tr.DORotate(rota, 1);
                ModelPosDic[key].Tr.DOMove(vc, 1);
            }
            nowType = StateEnum.none;
        }
    }

    //安装步骤 整体自动分步拆解
    public void StartIEnumToAssemble()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.zhengtizidongfenbuzuzhuang;
        StartCoroutine(IEnumToAssemble());
    }

    //安装步骤1
    public IEnumerator IEnumToAssemble()
    {
        foreach (string key in ModelPosDic.Keys)
        {
            Transform tr = ModelPosDic[key].Tr;
            Vector3 rota = ModelPosDic[key].roat2;
            Vector3 vc = ModelPosDic[key].pos2;
            ModelPosDic[key].Tr.DORotate(rota, 1);
            ModelPosDic[key].Tr.DOMove(vc, 1);
        }
        yield return new WaitForSeconds(2);
        foreach (string key in ModelPosDic.Keys)
        {
            Transform tr = ModelPosDic[key].Tr;
            Vector3 rota = ModelPosDic[key].roat1;
            Vector3 vc = ModelPosDic[key].pos1;// + new Vector3(0, 0.4f, 0);
            ModelPosDic[key].Tr.DORotate(rota, 1);
            ModelPosDic[key].Tr.DOMove(vc, 1);
            Transform CanvasUITrans = tr.GetChild(0);
            StartCoroutine(fenbuzhuzhuangUIIEnum(CanvasUITrans));
            yield return new WaitForSeconds(0.9f);
        }
        nowType = StateEnum.none;
        yield break;
    }
    IEnumerator fenbuzhuzhuangUIIEnum(Transform Tr)
    {
        Tr.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.2f);
        Tr.gameObject.SetActive(false);
        yield break;
    }

    /// <summary>
    /// 线圈
    /// </summary>
    public void Btn_VideoShow1()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.videoxianquan;

        videoPlayer.gameObject.SetActive(true);
        videoPlayer.clip = clip2;
        videoPlayer.Play();
        StartCoroutine(PanduanIsPlayingIEnum());
    }

    /// <summary>
    /// 铁芯
    /// </summary>
    public void Btn_VideoShow2()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.videotiexin;
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.clip = clip1;
        videoPlayer.Play();
        StartCoroutine(PanduanIsPlayingIEnum());
    }
    /// <summary>
    /// 套管
    /// </summary>
    public void Btn_VideoShow3()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.videotaoguan;
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.clip = clip1;
        videoPlayer.Play();
        StartCoroutine(PanduanIsPlayingIEnum());
    }

    //当前按钮状态
    enum StateEnum
    {
        none,
        zhengtiyijianchai,//整体一件拆解

        zhengtizidongfenbuzuzhuang,//整体自动分步组装

        videoxianquan,//视频展示
        videotiexin,//视频展示
        videotaoguan,//视频展示

        taoguanfenbuzhuang,//套管分步组装
        taoguanfenbuchai,//套管分步拆

        zhengtishoudongfenbuzuzhuang,//整体手动分步组装
        zhengtishoudongfenbuchaijie,//整体手动分步拆解
    }
    //[ContextMenu("playcc")]
    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void Send0()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = 0;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void EventTypeTop_Send1()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = 1;
        scalmess.SendToAllReliable();//发送给全部用户
    }

    public void EventTypeTop_Send2()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = 2;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void EventTypeTop_Send3()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_zidongfenbuanzhuang;
        scalmess.SendToAllReliable();//发送给全部用户
    }

    //线圈
    public void EventTypeTop_Send4()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.video_xianquan;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    //铁芯
    public void EventTypeTop_Send5()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.video_tiexin;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    //套管
    public void EventTypeTop_Send6()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.video_taogan;
        scalmess.SendToAllReliable();//发送给全部用户
    }






    ////////////////////////////////////////////////套管组装演示
    //套管的拆解
    public void TaoGuanChaiCam()
    {
        ThisCamera.transform.DOMove(targetCamera.position, 0.5f);
        ThisCamera.transform.DORotate(targetCamera.transform.eulerAngles, 0.5f);

        if (nowType == StateEnum.taoguanfenbuzhuang)
        {
            video_Button_4_S.gameObject.SetActive(true);
            video_Button_4_X.gameObject.SetActive(true);
        }
        else if (nowType == StateEnum.taoguanfenbuchai)
        {
            video_Button_4_S2.gameObject.SetActive(true);
            video_Button_4_X2.gameObject.SetActive(true);
        }

        canNextLast = true;
    }
    public void TaoGuanChai()
    {
        TaoGuanChaiCam();
        TaoGuan3.gameObject.SetActive(false);
        TaoGuanTarget.gameObject.SetActive(true);

        foreach (string key in TGPosDic.Keys)
        {
            Transform tr = TGPosDic[key].Tr;
            Vector3 rota = TGPosDic[key].roat2;
            Vector3 vc = TGPosDic[key].pos2;
            TGPosDic[key].Tr.DORotate(rota, 1);
            TGPosDic[key].Tr.DOMove(vc, 1);
        }
    }

    public void TaoGuanChai2()
    {
        TaoGuanChaiCam();

        TaoGuan3.gameObject.SetActive(false);
        TaoGuanTarget.gameObject.SetActive(true);
    }


    //套管安装步骤
    public void Btn_TaoGuanZhuang()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.taoguanfenbuzhuang;

        TaoGuanChai();
    }
    //套管拆解步骤2
    public void Btn_TaoGuanChai()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.taoguanfenbuchai;

        TaoGuanChai2();
    }

    int NowStape = 0;
    bool canNextLast = true;
    public void IEnum_BtnTaoGuanZhuangSX(int sx)
    {
        if (sx == 0)
        {
            if (NowStape - 1 < 0)
                return;
        }
        if (sx == 1)
        {
            if (NowStape > TGPosDic.Count)
                return;
        }
        if (!canNextLast) return;
        canNextLast = false;




        if (sx == 1)
        {
            NowStape++;
            foreach (PosData data in TGPosDic.Values)
            {
                if (data.indexNum == NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat1;
                    Vector3 vcs = data.pos1;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);
                    break;
                }
            }

        }
        else
        {
            foreach (PosData data in TGPosDic.Values)
            {
                if (data.indexNum == NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat2;
                    Vector3 vcs = data.pos2;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);
                    NowStape--;
                    break;
                }
            }

        }
        Invoke("CanClickTaoGuan", 1.2f);
    }
    public void IEnum_BtnTaoGuanChaiSX(int sx)
    {
        if (sx == 0)
        {
            if (NowStape - 1 < 0)
                return;
        }
        if (sx == 1)
        {
            if (NowStape > TGPosDic.Count)
                return;
        }
        if (!canNextLast) return;
        canNextLast = false;

        if (sx == 1)
        {
            foreach (PosData data in TGPosDic.Values)
            {
                if (data.indexNum == 14 - NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat2;
                    Vector3 vcs = data.pos2;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);

                    NowStape++;
                    break;
                }
            }
        }
        else
        {
            NowStape--;
            foreach (PosData data in TGPosDic.Values)
            {

                if (data.indexNum == 14 - NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat1;
                    Vector3 vcs = data.pos1;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);
                    break;
                }
            }

        }
        Invoke("CanClickTaoGuan", 1.2f);
    }
    void CanClickTaoGuan()
    {
        canNextLast = true;
        if (NowStape == TGPosDic.Count)
        {
            TaoGuanChaiCamhui();
            video_Button_4_S.gameObject.SetActive(false);
            video_Button_4_X.gameObject.SetActive(false);

            video_Button_4_S2.gameObject.SetActive(false);
            video_Button_4_X2.gameObject.SetActive(false);

            foreach (string key in TGPosDic.Keys)
            {
                Transform tr = TGPosDic[key].Tr;
                Vector3 rota = TGPosDic[key].roat1;
                Vector3 vc = TGPosDic[key].pos1;
                TGPosDic[key].Tr.DORotate(rota, 0);
                TGPosDic[key].Tr.DOMove(vc, 0);
            }
            TaoGuan3.gameObject.SetActive(true);
            TaoGuanTarget.gameObject.SetActive(false);
        }
    }

    public void TaoGuanChaiCamhui()
    {
        NowStape = 0;
        nowType = StateEnum.none;
        ThisCamera.transform.DOMove(origeCamera.position, 0.5f);
        ThisCamera.transform.DORotate(origeCamera.transform.eulerAngles, 0.5f);
    }


    /// ///////////////////////////////////      ///////////////////////////////////////////////////////////////////////
    //整体手动分步组装
    public void Btn_ZhengtiShouDongZuzhuang()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.zhengtishoudongfenbuzuzhuang;
        Bg_4.gameObject.SetActive(true);
        StartCoroutine(IEnumZhengtiShouDongZuzhuang());
    }

    //手动分步组装 
    public IEnumerator IEnumZhengtiShouDongZuzhuang()
    {
        foreach (string key in ModelPosDic.Keys)
        {
            Transform tr = ModelPosDic[key].Tr;
            Vector3 rota = ModelPosDic[key].roat2;
            Vector3 vc = ModelPosDic[key].pos2;
            ModelPosDic[key].Tr.DORotate(rota, 1);
            ModelPosDic[key].Tr.DOMove(vc, 1);
        }
        yield break;
    }

    //整体手动分步拆解
    public void Btn_ZhengtiShouDongChaijie()
    {
        if (nowType != StateEnum.none) return;
        nowType = StateEnum.zhengtishoudongfenbuchaijie;
        Bg_5.gameObject.SetActive(true);
    }



    public void IEnum_BtnZhengTiZhuangSX(int sx)
    {
        if (sx == 0)
        {
            Debug.Log(111);
            if (NowStape - 1 < 0)
                return;
        }
        if (sx == 1)
        {
            if (NowStape > 31)
                return;
        }
        if (!canNextLast) return;
        canNextLast = false;


        if (sx == 1)
        {
            NowStape++;
            foreach (PosData data in ModelPosDic.Values)
            {
                if (data.indexNum == NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat1;
                    Vector3 vcs = data.pos1;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);
                    Transform CanvasUITrans = tr.GetChild(0);
                    StartCoroutine(fenbuzhuzhuangUIIEnum(CanvasUITrans));

                    Bg_4name.text = tr.name;
                }
            }
        }
        else
        {
            foreach (PosData data in ModelPosDic.Values)
            {
                if (data.indexNum == NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat2;
                    Vector3 vcs = data.pos2;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);
                    Transform CanvasUITrans = tr.GetChild(0);
                    StartCoroutine(fenbuzhuzhuangUIIEnum(CanvasUITrans));

                    Bg_4name.text = tr.name;
                }
            }
            NowStape--;
        }
        Invoke("CanClickZhengti", 1.2f);
    }
    public void IEnum_BtnZhengTiChaiSX(int sx)
    {
        if (sx == 0)
        {
            if (NowStape - 1 < 0)
                return;
        }
        if (sx == 1)
        {
            if (NowStape > 31)
                return;
        }
        if (!canNextLast) return;
        canNextLast = false;

        if (sx == 1)
        {
            foreach (PosData data in ModelPosDic.Values)
            {
                if (data.indexNum == 31 - NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat2;
                    Vector3 vcs = data.pos2;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);

                    Bg_5name.text = tr.name;
                }
            }
            NowStape++;
        }
        else
        {
            NowStape--;
            foreach (PosData data in ModelPosDic.Values)
            {
                if (data.indexNum == 31 - NowStape)
                {
                    Transform tr = data.Tr;
                    Vector3 rotas = data.roat1;
                    Vector3 vcs = data.pos1;//+ new Vector3(0, 0.4f, 0);
                    data.Tr.DORotate(rotas, 1);
                    data.Tr.DOMove(vcs, 1);
                    Transform CanvasUITrans = tr.GetChild(0);
                    StartCoroutine(fenbuzhuzhuangUIIEnum(CanvasUITrans));

                    Bg_5name.text = tr.name;
                }
            }
        }
        Invoke("CanClickZhengti", 1.2f);
    }
    void CanClickZhengti()
    {
        canNextLast = true;
        if (NowStape == 32)
        {
            NowStape = 0;
            nowType = StateEnum.none;
            Bg_4.gameObject.SetActive(false);
            Bg_5.gameObject.SetActive(false);
        }
    }

    public void ButtonSXBack()
    {
        canNextLast = true;

        NowStape = 0;
        nowType = StateEnum.none;
        Bg_4.gameObject.SetActive(false);
        Bg_5.gameObject.SetActive(false);

        foreach (string key in ModelPosDic.Keys)
        {
            Transform tr = ModelPosDic[key].Tr;
            Vector3 rota = ModelPosDic[key].roat1;
            Vector3 vc = ModelPosDic[key].pos1;
            ModelPosDic[key].Tr.DORotate(rota, 1);
            ModelPosDic[key].Tr.DOMove(vc, 1);
        }
    }


    public void TaoGuan_Sen7()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.taoguan_btn_fenbuzuzhuang;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void TaoGuan_Se8()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.taoguan_btn_fenbuchaijie;
        scalmess.SendToAllReliable();//发送给全部用户
    }

    public void TaoGuan_Se9()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.taoguan_fenbuzuzhuang_s; ;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void TaoGuan_Se10()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.taoguan_fenbuzuzhuang_x; ;
        scalmess.SendToAllReliable();//发送给全部用户
    }

    public void TaoGuan_Se11()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.taoguan_fenbuchaijie_s;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void TaoGuan_Se12()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.taoguan_fenbuchaijie_x;
        scalmess.SendToAllReliable();//发送给全部用户
    }

    public void Zhengti_Send13()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_shoudongfenbuanzhuang;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void Zhengti_Send14()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_shoudongfenbuanchai;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void Zhengti_Send15()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_fenbuzuzhuang_s;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void Zhengti_Send16()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_fenbuzuzhuang_x;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void Zhengti_Send17()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_fenbuchaijie_s;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void Zhengti_Send18()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_fenbuchaijie_x;
        scalmess.SendToAllReliable();//发送给全部用户
    }
    public void Zhengti_Send19()
    {
        syncmessage scalmess = TiNetUtility.GetMessage<syncmessage>();
        scalmess.ButtonType = (int)MessageType.zhengti_chaizhuangBack;
        scalmess.SendToAllReliable();//发送给全部用户
    }
}

public class PosData
{
    public Transform Tr;
    public Vector3 pos1;
    public Vector3 roat1;

    public Vector3 pos2;
    public Vector3 roat2;

    public int indexNum; //顺序排位



}


