package com.horus.horusmobile;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import java.util.ArrayList;

/**
 * Created by Timothy on 19/08/2016.
 */
public class ResultsAdapter extends ArrayAdapter<IEvent> {
    private final Context context;
    private final ArrayList<IEvent> results;

    public ResultsAdapter(Context context, ArrayList<IEvent> results) {
        super(context, R.layout.activity_main, results);
        this.context = context;
        this.results = results;
    }

    @Override
    public View getView(int position, View convertview, ViewGroup parent) {
        LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View rowView = inflater.inflate(R.layout.item_layout, parent, false);

        TextView textLine1 = (TextView) rowView.findViewById(R.id.event);
        TextView textLine2 = (TextView) rowView.findViewById(R.id.description);

        IEvent item = this.results.get(position);

        textLine1.setText(item.Created.toString());
        textLine2.setText("IP: " + item.Description.toString());

        return rowView;
    }
}
