"""
VisionOS App Lifecyle Test Helper
This script exists to facilitate the tests in AppLifecycleTests.cs when they are running on visionOS. Because there is no way for a
visionOS (or iOS) app to bring itself into the foreground, we must rely on an external script. This will be run automatically by the Editor
TestRunner thanks to AppLifecycleTestsEditorHelper.cs, but it must be run manually for standalone UTR test runs on CI. If this script isn't
running, the tests will time out waiting for the app to be brought back to the foreground. In order to prevent this from interfering with
the startup and shutdown of the test player, this script will wait until it detects the app is running on the booted simulator, and then it
will foreground (launch) the app every 10 seconds until it is no longer running.

The script requires two arguments:
- The app name, for example PolySpatialTest.app.
- The bundle identifier, for example com.Unity.PolySpatialTest.

At the beginning of each cycle, we check  if the app is running, we run the command `ps aux | pgrep -fl <app_name>`. Although we need to
manipulate the app state on the simulator, the app itself is still a macOS process that we can detect with normal shell commands. If the
app is running, we set HAS_LAUNCHED to true and move on to foregrounding it. This command is` xcrun simctl launch booted <bundle_id>`.

`xcrun` is a general-purpose utility shipped with Xcode which provides access to various parts of the toolchain via command line interface.
`simctl` is one of these tools which can control the Simulator app. The `launch` command will launch an app as if the user clicked its icon
on the home screen. `booted` is a wildcard that allows us to run commands on any currently running virtual device without having to know
the specific device ID.

We want the script to terminate itself after the test run is complete. That's what HAS_LAUNCHED is for. If we have detected the running
app, and we no longer detect that it is running, we can assume the test run is complete and the script will terminate.

The script will send any other running instances of itself (identified by the using script's own name in another call to `ps`) the SIGTERM
signal, requesting a clean shutdown. We only want one of these running in the background or they may send launch commands too frequently.
"""

import subprocess
import signal
import time
import sys
import os

# Time to wait between checks (in seconds).
SLEEP_TIME = 10

# Indicates that the app has been launched at least once.
HAS_LAUNCHED = False

def print_flush(message):
    """Custom print function that prepends the message with a label and flushes output immediately."""
    print(f"App Lifecycle Helper Script: {message}", flush=True)

def terminate_self():
    """Announce that the script will terminate and exit."""
    print_flush("Terminate Self.")
    exit()

def bring_app_to_foreground(app_name, bundle_id):
    """Bring the app with id app_name to the foreground if it is already running."""

    global HAS_LAUNCHED


    # Check if the app is running before trying to bring it to the foreground.
    try:
        result = subprocess.run("ps aux | pgrep -fl " + app_name, check=True, shell=True, capture_output=True, text=True)

        app_is_running = False
        for line in result.stdout.splitlines():
            try:
                pid_str, command = line.strip().split(None, 1)

                # If we find a command with "CoreSimulator" and the app name the app is running.
                # The path to the version of the app installed to the simulator will always contain "CoreSimulator". This lets us
                # distinguish the actual command used to run the app from the command to run this script or install the app.
                if app_name in line and "CoreSimulator" in line:
                    print_flush(f"Found running app: {line}")
                    app_is_running = True
            except ValueError:
                continue  # skip malformed lines

        if not app_is_running:
            # If the app is not running and we've already launched it once, we're done.
            if HAS_LAUNCHED:
                terminate_self()

            return

    except subprocess.CalledProcessError as e:
        # Suppress errors, and continue to check if the app is running.
        if HAS_LAUNCHED:
            terminate_self()

        return


    """
    If we've gotten this far, the app is running and we're going to try to launch it. Set HAS_LAUNCHED to true and wait for the next cycle
    in case the app is still starting up. Now that HAS_LAUNCHED is true, we will stop the cycle next time we detect that the app is no
    longer running. A normal player test run does not close the app until all tests are complete.
    Note that this may not work properly for UTR re-runs, which will launch the app more than once from the same command. Our package CI
    disables this feature, but we will need to make changes to this test suite if we want to support re-runs.
    """
    if not HAS_LAUNCHED:
        HAS_LAUNCHED = True
        return

    print_flush(f"Has detected running app. Trying to launch {bundle_id}")

    # Bring the app to the foreground using simctl launch.
    try:
        subprocess.run(["xcrun", "simctl", "launch", "booted", bundle_id], check=True)
        print_flush(f"App {bundle_id} brought to foreground.")

    except subprocess.CalledProcessError as e:
        print_flush(f"Failed to bring app to foreground: {e}")

def kill_old_instances(script_name):
    current_pid = os.getpid()
    result = subprocess.run(
        ["ps", "axo", "pid,command"],
        stdout=subprocess.PIPE,
        stderr=subprocess.DEVNULL,
        text=True
    )

    for line in result.stdout.splitlines():
        try:
            pid_str, command = line.strip().split(None, 1)
            pid = int(pid_str)
            if pid != current_pid and script_name in command:
                print(f"Killing old instance: PID {pid}")
                os.kill(pid, signal.SIGTERM)  # or SIGKILL if necessary
        except ValueError:
            continue

def main():
    if len(sys.argv) != 3:
        print("Usage: python app_lifecycle_test_helper.py <app_name> <bundle_id>")
        print("Example: python app_lifecycle_test_helper.py TestProject.app com.unity.TestProject")
        return

    kill_old_instances("app-lifecycle-test-helper.py")
    app_name = sys.argv[1]
    bundle_id = sys.argv[2]

    while True:
        bring_app_to_foreground(app_name, bundle_id)
        time.sleep(SLEEP_TIME)

if __name__ == "__main__":
    main()
