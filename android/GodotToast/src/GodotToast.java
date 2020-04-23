package org.godotengine.godot;

import android.app.Activity;
import android.app.AlertDialog;
import android.os.Handler;
import android.os.Message;
import android.content.DialogInterface;
import android.util.Log;
import android.widget.Toast;
import android.content.Intent;


public class GodotToast extends Godot.SingletonBase 
{   private static final String TAG = "godot";
    protected Activity activity = null; 

    public void sendToast(final String mess)
	{
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Toast.makeText(activity, mess, Toast.LENGTH_LONG).show();
            };
                          
		});
	}
    /**
     * Internal callbacks
     */

    @Override
    protected void onMainActivityResult(int requestCode, int resultCode, Intent data) {

                   
                if(resultCode == Activity.RESULT_OK) {
                    Log.d(TAG,  "Sent Activated!");
					
                }
                else {
                    Log.d(TAG, "Not Send");
                }
                
            }

    /**
     * Initilization of the Singleton
     */

    static public Godot.SingletonBase initialize(Activity p_activity)
    {
        return new GodotToast(p_activity);
    }

    /**
     * Constructor
     */

    public GodotToast (Activity activity) 
    {
        registerClass("GodotToast", new String[]
        {
            "sendToast"
        });

        this.activity = activity;
    }
}
