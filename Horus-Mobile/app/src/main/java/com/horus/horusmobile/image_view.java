package com.horus.horusmobile;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.webkit.WebView;

public class image_view extends AppCompatActivity {

    WebView webView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_image_view);

        webView = (WebView)findViewById(R.id.webView);

        String url = getIntent().getExtras().getString("url");

        webView.loadUrl(url);
    }
}
