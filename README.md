# Project-Horus
Web cam security app

Project Horus – a prototype of a desktop security application.

Face Identification (or “Recognition”)

This project is intended to demonstrate several features typically found in security/surveillance applications that utilise computer vision. The idea of the project is that when the user is away from their server or desktop computer a screensaver is activated and the webcam monitors the environment for unauthorised people. If someone is at the desktop who is unauthorised, the application notifies the registered user by sms and makes available a photograph of the unauthorised person to a mobile client application. However if it is the registered user who appears in the immediate environment then the screensaver is de-activated.  The key to this functionality is what is called ‘facial recognition’.

We have used Micrsoft’s Face API available through Microsoft Cognitive Services. Microsoft Cognitive Services enables anyone to build applications with powerful algorithms using just a few lines of code. The services work across devices and platforms such as iOS, Android, and Windows, keep improving, and are easy to set up.

Person Group

A person group is one of the most important parameters for the Face - Identify API which we have used from Microsoft Cognitive Services. 

Microsoft Cognitive Services’ Face API can be used to identify people based on a detected face and people database (defined as a person group) which needs to be created in advance and can be edited over time.

The following figure is an example of a person group named "myfriends". Each group may contain up to 1,000 person objects. Meanwhile, each person object can have one or more faces registered. The more faces registered for each person object, the more accurate is the identification service provided. The suggested number of photographs for the purpose of this project has been five.





Person Group


![alt tag](https://portalstoragewuprod.azureedge.net/media/Default/Documentation/Face/Images/person.group.clare.jpg)

After a person group has been created and trained, identification can be performed against the group and a new detected face in the webcam. If the face is identified as a person object in the group, the person object will be returned.

The Video

The following video demonstrates a logical workflow, taking the user through a series of steps, from installation and finally to facial recognition and application termination.


Installation.

Download and open the executable at: https://drive.google.com/drive/folders/0B2l2uPgKwO5eQk5PT0lPV1o0MVk


Enter user name and email. 

Open email link in email client. 

Download and install the Android application package downloadable at the provided url in the email.

Double-click on the application’s desktop icon.

Enter user details (name, telecommunications provider, etc) at the User Interface.

If the user is subsequently detected the screensaver will close – otherwise it will run and if someone who is not a registered user appears in the immediate environment the application’s owner will be notified by SMS.  The owner can then access the photograph of the event.via the mobile client.
