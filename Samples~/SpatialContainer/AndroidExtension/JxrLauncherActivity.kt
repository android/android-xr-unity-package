package com.unity3d.launcher

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.xr.compose.spatial.Subspace
import androidx.xr.compose.subspace.SpatialActivityPanel
import androidx.xr.compose.subspace.SpatialPanel
import androidx.xr.compose.subspace.layout.SubspaceModifier
import androidx.xr.compose.subspace.layout.movable
import androidx.xr.compose.subspace.layout.resizable
import androidx.xr.compose.subspace.layout.width
import androidx.xr.compose.subspace.layout.height
import androidx.xr.compose.subspace.layout.offset
import com.unity3d.player.UnityPlayer
import com.unity3d.player.UnityPlayerGameActivity
import kotlinx.coroutines.delay

class JxrLauncherActivity : ComponentActivity() {
  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)

    application.registerActivityLifecycleCallbacks(
      object : android.app.Application.ActivityLifecycleCallbacks {
        override fun onActivityCreated(
          activity: android.app.Activity,
          savedInstanceState: Bundle?,
        ) {
          if (activity.javaClass.simpleName == "UnityPlayerGameActivity") {

            activity.window.setBackgroundDrawable(
              android.graphics.drawable.ColorDrawable(android.graphics.Color.TRANSPARENT)
            )
            activity.window.addFlags(
              android.view.WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE or
                android.view.WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
            )
          }
        }

        override fun onActivityStarted(activity: android.app.Activity) {}

        override fun onActivityResumed(activity: android.app.Activity) {}

        override fun onActivityPaused(activity: android.app.Activity) {}

        override fun onActivityStopped(activity: android.app.Activity) {}

        override fun onActivitySaveInstanceState(
          activity: android.app.Activity,
          outState: Bundle,
        ) {}

        override fun onActivityDestroyed(activity: android.app.Activity) {}
      }
    )

    setContent { MainScreen() }
  }
}

@Composable
fun MainScreen() {
  val context = LocalContext.current

  var isPanelExperienceStarted by remember { mutableStateOf(false) }
  var isStandaloneExperienceStarted by remember { mutableStateOf(false) }
  val anyExperienceStarted = isPanelExperienceStarted || isStandaloneExperienceStarted

  var areButtonsEnabled by remember { mutableStateOf(false) }
  var inputText by remember { mutableStateOf("") }

  LaunchedEffect(anyExperienceStarted) {
    if (anyExperienceStarted) {
      delay(5000)
      areButtonsEnabled = true
    }
  }

  Subspace {
    SpatialPanel(modifier = SubspaceModifier.movable().resizable().width(800.dp).height(1100.dp)) {
      Column(
        modifier = Modifier.fillMaxSize().background(Color.White).padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center,
      ) {
        TextField(
          value = inputText,
          onValueChange = { inputText = it },
          placeholder = { Text("Enter text here...") },
        )

        Spacer(modifier = Modifier.height(30.dp))

        Button(onClick = { isPanelExperienceStarted = true }, enabled = !anyExperienceStarted) {
          Text("Start Unity Experience (Spatial Activity Panel)")
        }

        Spacer(modifier = Modifier.height(15.dp))

        Button(
          onClick = {
            isStandaloneExperienceStarted = true
            val intent = Intent(context, UnityPlayerGameActivity::class.java)
            context.startActivity(intent)
          },
          enabled = !anyExperienceStarted,
        ) {
          Text("Start Unity Experience (Standalone)")
        }

        Spacer(modifier = Modifier.height(15.dp))

        Button(
          onClick = { UnityPlayer.UnitySendMessage("Container", "BugdroidWave", "") },
          enabled = areButtonsEnabled,
        ) {
          Text("Wave Bugdroid!")
        }

        Spacer(modifier = Modifier.height(15.dp))

        Button(
          onClick = { UnityPlayer.UnitySendMessage("Container", "BugdroidImmersive", "") },
          enabled = areButtonsEnabled,
        ) {
          Text("Immersive Mode")
        }

        Spacer(modifier = Modifier.height(15.dp))

        Button(
          onClick = { UnityPlayer.UnitySendMessage("Container", "BugdroidToggleVisibility", "") },
          enabled = areButtonsEnabled,
        ) {
          Text("Toggle Visibility")
        }
      }
    }

    if (isPanelExperienceStarted) {
      SpatialActivityPanel(intent = Intent(context, UnityPlayerGameActivity::class.java))

      SpatialPanel(modifier = SubspaceModifier.movable().offset(x = -300.dp, z = 2.dp)) {
        Box(
          modifier = Modifier.background(Color.Blue).padding(24.dp),
          contentAlignment = Alignment.Center,
        ) {
          Text(
            text = "✦ Spatial Info Panel ✦\nHosted in the same 3D Scene Root!",
            color = Color.White,
            fontSize = 20.sp,
            textAlign = TextAlign.Center,
          )
        }
      }
    }
  }
}
