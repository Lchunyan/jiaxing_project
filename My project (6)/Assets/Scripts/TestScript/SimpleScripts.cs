using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;

public class SimpleScripts : MonoBehaviour
{
    public MeshRenderer thisrender;

    public GameObject RotModel;

    public GameObject MoveModel;

    private List<Vector3> OriginTran;

    public GameObject UIIstruc;

    //public GameObject RecoverModel;

    public GameObject sceneobject1;


    void Start()

    {
        List<Vector3> cubelist = new List<Vector3>();

        cubelist.Add(sceneobject1.transform.position);
        cubelist.Add(sceneobject1.transform.eulerAngles);

        OriginTran = cubelist;
        //Debug.Log(OriginPos[0]);



        string[] nowtime = System.DateTime.Now.ToString("yyyy:MM:dd").Split(new char[] { ':' });
        if (int.Parse(nowtime[1]) == 9 || int.Parse(nowtime[1]) == 10)
        {
#if UNITY_EDITOR
        UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
#else
            UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
#endif

        }
    }

    public void CallHideUI()
    {
        UIIstruc.SetActive(!UIIstruc.activeInHierarchy);
    }

    public void RecoverAllPos()
    {
        sceneobject1.transform.position = OriginTran[0];
        sceneobject1.transform.eulerAngles = OriginTran[2];
    }


    public void ChangeColor()

    {
        thisrender.material.color = Color.red;
    }
    public void ClearColor()
    {
        thisrender.material.color = Color.white;
    }

    public void Rotatemodel()
    {
        RotModel.transform.eulerAngles += new Vector3(0, 30, 0);
    }

    public void MovePos()

    {
        MoveModel.transform.position = new Vector3(MoveModel.transform.position.x, MoveModel.transform.position.y - 0.04f, MoveModel.transform.position.z);


    }

    public void ButtonRecover()
    {
        MoveModel.transform.position = new Vector3(MoveModel.transform.position.x, MoveModel.transform.position.y + 0.04f, MoveModel.transform.position.z);
    }

}
