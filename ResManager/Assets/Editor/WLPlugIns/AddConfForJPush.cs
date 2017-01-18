using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class AddConfForJPush : MonoBehaviour {

	// Use this for initialization

    public Dictionary<string,string> packageToKey = new Dictionary<string,string>();
    public Dictionary<string,string> chanels = new Dictionary<string,string>();
    public Dictionary<string,string> appkeys = new Dictionary<string,string>();

    public static string perReplaceTag = "uses-permission-stop-->";
    public static string actReplaceTag = "activity-stop-->";
    public static string perContent = @"




	<!-- 如下内容放到AndroidManifest.xml的<application>标签的外面 -->
	<!-- lebian sdk permission begin -->
    <uses-permission android:name=""android.permission.INTERNET""/>
    <uses-permission android:name=""android.permission.WAKE_LOCK"" />
    <uses-permission android:name=""android.permission.ACCESS_NETWORK_STATE""/>
    <uses-permission android:name=""android.permission.ACCESS_WIFI_STATE""/>
    <uses-permission android:name=""android.permission.WRITE_EXTERNAL_STORAGE""/>
    <uses-permission android:name=""android.permission.GET_TASKS"" />
    <uses-permission android:name=""android.permission.READ_PHONE_STATE""/>
    <uses-permission android:name=""com.android.launcher.permission.INSTALL_SHORTCUT""/>
	<!-- lebian sdk permission end -->





    <!--PUSH-permission-start-->
    <permission
        android:name=""<PACKAGE>.permission.JPUSH_MESSAGE""
        android:protectionLevel=""signature"" />

    <!-- Required  一些系统要求的权限，如访问网络等-->
    <uses-permission android:name=""<PACKAGE>.permission.JPUSH_MESSAGE"" />
    <uses-permission android:name=""android.permission.RECEIVE_USER_PRESENT"" />
    <uses-permission android:name=""android.permission.INTERNET"" />
    <uses-permission android:name=""android.permission.WAKE_LOCK"" />
    <uses-permission android:name=""android.permission.READ_PHONE_STATE"" />
    <uses-permission android:name=""android.permission.WRITE_EXTERNAL_STORAGE"" />
    <uses-permission android:name=""android.permission.READ_EXTERNAL_STORAGE"" />
    <uses-permission android:name=""android.permission.WRITE_SETTINGS"" />
    <uses-permission android:name=""android.permission.VIBRATE"" />
    <uses-permission android:name=""android.permission.MOUNT_UNMOUNT_FILESYSTEMS"" />
    <uses-permission android:name=""android.permission.ACCESS_NETWORK_STATE"" />
    <uses-permission android:name=""android.permission.SYSTEM_ALERT_WINDOW""/>  

    
    
    <!-- Optional for location -->
    <uses-permission android:name=""android.permission.ACCESS_COARSE_LOCATION"" />
    <uses-permission android:name=""android.permission.ACCESS_WIFI_STATE"" />
    <uses-permission android:name=""android.permission.CHANGE_WIFI_STATE"" />
    <uses-permission android:name=""android.permission.ACCESS_FINE_LOCATION"" />
    <uses-permission android:name=""android.permission.ACCESS_LOCATION_EXTRA_COMMANDS"" />
    <uses-permission android:name=""android.permission.CHANGE_NETWORK_STATE"" />

    <!--PUSH-permission-stop-->

    ";

    public static string actContent = @"

        <!--PUSH-activity-start-->
        <!-- Required SDK核心功能-->
        <activity
            android:name=""cn.jpush.android.ui.PushActivity""
            android:theme=""@android:style/Theme.Translucent.NoTitleBar""
            android:configChanges=""orientation|keyboardHidden"" >
            <intent-filter>
                <action android:name=""cn.jpush.android.ui.PushActivity"" />
                <category android:name=""android.intent.category.DEFAULT"" />
                <category android:name=""<PACKAGE>"" />
            </intent-filter>
        </activity>
        <!-- Required  SDK核心功能-->
        <service
            android:name=""cn.jpush.android.service.DownloadService""
            android:enabled=""true""
            android:exported=""false"" >
        </service>
    
        
        <!-- Required SDK 核心功能-->
        <service
            android:name=""cn.jpush.android.service.PushService""
            android:enabled=""true""
            android:exported=""false"">
            <intent-filter>
                <action android:name=""cn.jpush.android.intent.REGISTER"" />
                <action android:name=""cn.jpush.android.intent.REPORT"" />
                <action android:name=""cn.jpush.android.intent.PushService"" />
                <action android:name=""cn.jpush.android.intent.PUSH_TIME"" />
                
            </intent-filter>
        </service>
        
        <!-- Required SDK核心功能-->
        <receiver
            android:name=""cn.jpush.android.service.PushReceiver""
            android:enabled=""true"" >
             <intent-filter android:priority=""1000"">
                <action android:name=""cn.jpush.android.intent.NOTIFICATION_RECEIVED_PROXY"" />   <!--Required  显示通知栏 -->
                <category android:name=""<PACKAGE>"" />
            </intent-filter>
            <intent-filter>
                <action android:name=""android.intent.action.USER_PRESENT"" />
                <action android:name=""android.net.conn.CONNECTIVITY_CHANGE"" />
            </intent-filter>
             <!-- Optional -->
            <intent-filter>
                <action android:name=""android.intent.action.PACKAGE_ADDED"" />
                <action android:name=""android.intent.action.PACKAGE_REMOVED"" />
                <data android:scheme=""package"" />
            </intent-filter>
   
        </receiver>
        
        <!-- Required SDK核心功能-->
        <receiver android:name=""cn.jpush.android.service.AlarmReceiver"" />
        
        <!-- User defined.  For test only  用户自定义的广播接收器-->
        <receiver
            android:name=""com.hdls.fgame.jpush.MyReceiver""
            android:enabled=""true"">
            <intent-filter>
                <action android:name=""cn.jpush.android.intent.REGISTRATION"" /> <!--Required  用户注册SDK的intent-->
                <action android:name=""cn.jpush.android.intent.UNREGISTRATION"" />  
                <action android:name=""cn.jpush.android.intent.MESSAGE_RECEIVED"" /> <!--Required  用户接收SDK消息的intent-->
                <action android:name=""cn.jpush.android.intent.NOTIFICATION_RECEIVED"" /> <!--Required  用户接收SDK通知栏信息的intent-->
                <action android:name=""cn.jpush.android.intent.NOTIFICATION_OPENED"" /> <!--Required  用户打开自定义通知栏的intent-->
                <action android:name=""cn.jpush.android.intent.ACTION_RICHPUSH_CALLBACK"" /> <!--Optional 用户接受Rich Push Javascript 回调函数的intent-->
                <category android:name=""<PACKAGE>"" />
            </intent-filter>
        </receiver>
 
        
        <!-- Required  . Enable it you can get statistics data with channel -->
        <meta-data android:name=""JPUSH_CHANNEL"" android:value=""<CHANNEL>""/>
        <meta-data android:name=""JPUSH_APPKEY"" android:value=""<APPKEY>"" /> <!--  </>值来自开发者平台取得的AppKey-->
        
        <!--PUSH-activity-stop-->







	<!-- 如下内容放到AndroidManifest.xml的<application>标签的里面 -->
	<!-- lebian sdk components begin -->
	<meta-data android:name=""ClientChId"" android:value=""<CHANNEL>"" />
	<meta-data android:name=""MainChId"" android:value=""60061"" />

	<activity android:name=""com.excelliance.open.KXQP""
		android:process="":lbmain""
		android:screenOrientation=""landscape""
		android:theme=""@android:style/Theme.NoTitleBar.Fullscreen""
		android:configChanges=""orientation|screenSize"">
		<intent-filter>
			<!--请勿删除此intent filter，如果其余SDK要求设置他们的activity为主activity，只需将下面的meta data配置为他们的主activity即可-->
			<action android:name=""android.intent.action.MAIN"" />
			<category android:name=""android.intent.category.LAUNCHER"" />
		</intent-filter>
		<meta-data android:name=""mainActivity"" android:value=""com.hdls.fgame.sdk.UnityPlayerActivity"" >
		</meta-data>
	</activity>
	<activity android:name=""com.excelliance.open.platform.NextChapter""
			android:process="":lbmain""
		android:screenOrientation=""landscape""
		android:theme=""@style/lebian_main_app_theme""
		android:configChanges=""orientation|screenSize"">
		<intent-filter>
			<action android:name=""com.excelliance.open.action.startNextChapter""/>
			<category android:name=""android.intent.category.DEFAULT"" />
		</intent-filter>
	</activity>
	
	<activity android:name=""com.excelliance.open.PromptActivity""
		android:process="":lbmain""
		android:screenOrientation=""landscape""
		android:theme=""@android:style/Theme.Translucent.NoTitleBar""
		android:configChanges=""orientation|screenSize"">
		<intent-filter>
			<action android:name=""com.excelliance.open.action.startPromptActivity""/>
			<category android:name=""android.intent.category.DEFAULT"" />
		</intent-filter>
	</activity>
	
	<receiver android:name=""com.excelliance.open.notification.BGReceiver"" android:process="":lbmain"">
	  <intent-filter>
		<action android:name=""android.net.conn.CONNECTIVITY_CHANGE"" />
	  </intent-filter>
	  <intent-filter>
		<action android:name=""com.excelliance.open.action.appstate"" />
	  </intent-filter>
	  <intent-filter>
		<action android:name=""com.excelliance.open.action.queryUpdate"" />
	  </intent-filter>
	  <intent-filter>
		  <action android:name=""android.intent.action.MEDIA_MOUNTED"" />
		  <data android:scheme=""file"" />
	  </intent-filter>
	</receiver>
	<service android:name=""com.excelliance.open.BGService""
		android:process="":download"">
		<intent-filter>
			<action android:name=""com.excelliance.open.action.gameverchk""/>
			<action android:name=""com.excelliance.open.action.apkverchk""/>
			<action android:name=""com.excelliance.open.action.dmchk""/>
			<action android:name=""com.excelliance.open.action.fw""/>
		</intent-filter>
		<intent-filter>
			<action android:name=""com.excelliance.open.NEXT_CHAPTER"" />
			<action android:name=""com.excelliance.open.action.PLAT_DO"" />
		</intent-filter>
	</service>

	<activity android:name=""com.excelliance.kxqp.platform.GameActivity""
			android:process="":lbmain""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"">
	</activity>
	
	<service android:name=""com.android.ggapsvc.LBService""
		android:process="":lebian"">
		<intent-filter>
			<action android:name=""com.excelliance.open.action.actlbs1""/>
			<action android:name=""com.excelliance.open.action.actlbs2""/>
			<action android:name=""com.excelliance.open.action.actlbs3""/>
			<action android:name=""com.excelliance.open.action.ACT_LBService""/>
		</intent-filter>
	</service>
	
	<receiver android:name=""com.excelliance.kxqp.notification.NotificationReceiver"" android:process="":lebian"" >
	  <intent-filter>
		<action android:name=""android.net.conn.CONNECTIVITY_CHANGE"" />
	  </intent-filter>
	  <intent-filter>
		  <action android:name=""com.excelliance.open.action.ACT_LB_RECEIVER""/>
		  <action android:name=""com.excelliance.open.action.downloadcomponent.progress""/>
	  </intent-filter>
	</receiver>
	
	<service android:name=""com.excelliance.kxqp.platform.PlatformService"" android:process="":lbmain"">
		<intent-filter>
			<action android:name=""com.excelliance.kxqp.platform.gameplugin.action.BIND_REMOTE"" />
			<category android:name=""android.intent.category.DEFAULT"" />
		</intent-filter>
		<intent-filter>
			<action android:name=""com.excelliance.kxqp.platform.gameplugin.action.UNBIND_REMOTE"" />
			<category android:name=""android.intent.category.DEFAULT"" />
		</intent-filter>
		<intent-filter>
			<action android:name=""com.excelliance.open.platform.gameplugin.action.START_FROM_SHORTCUT"" />
			<category android:name=""android.intent.category.DEFAULT"" />
		</intent-filter>
		<intent-filter>
			<action android:name=""com.excelliance.open.action.ACTIVITY_STATE"" />
			<category android:name=""android.intent.category.DEFAULT"" />
		</intent-filter>
	</service>
	<service android:name=""com.excelliance.open.PrestartService"" />
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.BootService"" >
	</service>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxy""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxy1""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxy2""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxy3""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxy4""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxy5""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxyT""
		android:theme=""@style/lebian_activity_proxy_t""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxyT1""
		android:theme=""@style/lebian_activity_proxy_t""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxyT2""
		android:theme=""@style/lebian_activity_proxy_t""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxySI1""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleInstance"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxySI2""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleInstance"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxySIT1""
		android:theme=""@style/lebian_activity_proxy_t""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleInstance"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxyST1""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleTask"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxyST2""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleTask"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxySTT1""
		android:theme=""@style/lebian_activity_proxy_t""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleTask"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxySTop1""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleTop"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxySTop2""
		android:theme=""@style/lebian_activity_proxy""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleTop"" >
	</activity>
	<activity
		android:name=""com.excelliance.kxqp.platform.gameplugin.ActivityProxySTopT1""
		android:theme=""@style/lebian_activity_proxy_t""
		android:screenOrientation=""landscape""
		android:configChanges=""orientation|screenSize""
		android:launchMode=""singleTop"" >
	</activity>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy1"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy2"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy3"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy4"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy5"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy6"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy7"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy8"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy9"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy10"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_A1""
		android:process="":platform.gameplugin_SP_A"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_A2""
		android:process="":platform.gameplugin_SP_A"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_A3""
		android:process="":platform.gameplugin_SP_A"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_A4""
		android:process="":platform.gameplugin_SP_A"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_B1""
		android:process="":platform.gameplugin_SP_B"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_B2""
		android:process="":platform.gameplugin_SP_B"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_B3""
		android:process="":platform.gameplugin_SP_B"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_B4""
		android:process="":platform.gameplugin_SP_B"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_C1""
		android:process="":platform.gameplugin_SP_C"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_C2""
		android:process="":platform.gameplugin_SP_C"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_C3""
		android:process="":platform.gameplugin_SP_C"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_C4""
		android:process="":platform.gameplugin_SP_C"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_D1""
		android:process="":platform.gameplugin_SP_D"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_D2""
		android:process="":platform.gameplugin_SP_D"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_D3""
		android:process="":platform.gameplugin_SP_D"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_D4""
		android:process="":platform.gameplugin_SP_D"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_E1""
		android:process="":platform.gameplugin_SP_E"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_E2""
		android:process="":platform.gameplugin_SP_E"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_E3""
		android:process="":platform.gameplugin_SP_E"" >
	</service>
	<service android:name=""com.excelliance.kxqp.platform.gameplugin.ServiceProxy_SP_E4""
		android:process="":platform.gameplugin_SP_E"" >
	</service>
	<receiver android:name=""com.excelliance.kxqp.platform.gameplugin.ReceiverProxy"" >
	  <intent-filter>
		<action android:name=""rpa.com.excelliance.kxqp.platform.gameplugin.ReceiverProxy"" />
	  </intent-filter>
	</receiver>
	<receiver android:name=""com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_A""
		android:process="":platform.gameplugin_SP_A"" >
	  <intent-filter>
		<action android:name=""rpa.com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_A"" />
	  </intent-filter>
	</receiver>
	<receiver android:name=""com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_B""
		android:process="":platform.gameplugin_SP_B"" >
	  <intent-filter>
		<action android:name=""rpa.com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_B"" />
	  </intent-filter>
	</receiver>
	<receiver android:name=""com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_C""
		android:process="":platform.gameplugin_SP_C"" >
	  <intent-filter>
		<action android:name=""rpa.com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_C"" />
	  </intent-filter>
	</receiver>
	<receiver android:name=""com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_D""
		android:process="":platform.gameplugin_SP_D"" >
	  <intent-filter>
		<action android:name=""rpa.com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_D"" />
	  </intent-filter>
	</receiver>
	<receiver android:name=""com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_E""
		android:process="":platform.gameplugin_SP_E"" >
	  <intent-filter>
		<action android:name=""rpa.com.excelliance.kxqp.platform.gameplugin.ReceiverProxy_SP_E"" />
	  </intent-filter>
	</receiver>	
	<!-- lebian sdk components end -->




";

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void AddConfForJushByChannel(string channel,string package,string appkey)
    {
        string configFilePath = Application.dataPath + "/../OtherSdk/" + channel.ToLower() + "/Android/AndroidManifest.xml";
        AddConfForJushByChannel(configFilePath,channel,package,appkey);
        configFilePath = Application.dataPath + "/../OtherSdk/" + channel.ToLower() + "/AndroidEclipse/AndroidManifest.xml";
        AddConfForJushByChannel(configFilePath, channel, package, appkey);

    }

    public static void AddConfForJushByChannel(string configFilePath, string channel, string package, string appkey)
    {
        if (File.Exists(configFilePath) == false)
        {
            Debug.LogError("file doesnot exists at path " + configFilePath);
            return;
        }

        string curPerContent = perContent.Replace("<PACKAGE>", package);
        string curActContent = actContent.Replace("<PACKAGE>", package).Replace("<CHANNEL>", channel).Replace("<APPKEY>", appkey);

        Debug.LogError(curPerContent);
        Debug.LogError(curActContent);

        string configContent = File.ReadAllText(configFilePath);

        int perIndex = configContent.IndexOf(perReplaceTag);
        int actIndex = configContent.IndexOf(actReplaceTag);


        string configContent1 = configContent.Substring(0, perIndex + perReplaceTag.Length);
        string configContent2 = configContent.Substring(perIndex + perReplaceTag.Length, actIndex + actReplaceTag.Length - (perIndex + perReplaceTag.Length));
        string configContent3 = configContent.Substring(actIndex + actReplaceTag.Length);

        configContent = configContent1 + " \n" + curPerContent + "\n" + configContent2 + "\n" + curActContent + "\n" + configContent3;

        Debug.LogError("configContent" + "\n" + configContent);

        File.WriteAllText(configFilePath, configContent, System.Text.Encoding.UTF8);
    }
}
