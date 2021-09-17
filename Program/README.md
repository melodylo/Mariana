# Project Mariana
Project Mariana allows you to plot and interact with data from a SQL database.

## Technologies
* MySql.Data Version 8.0.26
* ScottPlot Version 4.1.17

## Features
* Plot data from a SQL database
  * Can specify a minimum and/or maximum value for the x-axis
  * Can plot to two different y-axes (primary and secondary)
    * Can change scaling of y-axis (linear or logarithmic)
* Show the nearest (x, y) coordinates of the nearest plotted line to the mouse cursor 
  * Can turn on/off
* Interact with the plotted data (see [ScottPlot](https://github.com/ScottPlot/ScottPlot#readme) for more details)
  * Pan the plot in any direction
  * Zoom in/out
  * Adjust scaling of x and y-axis
  * Zoom into selected region
  * Fit the plotted lines to plot
* Hide/show plotted lines
* Delete plotted lines
* Clear plotted lines
* Add data to SQL table (only small amounts of data is recommended)
  * Can manually add rows of data or import CSV file 
  * Can update plotted lines if data was successfully added
* Add column to SQL table
  * Supports int, float, datetime, and varchar
* Export/archive data from SQL table into a CSV file
  * Allows multiple constraints with and/or logic
  * Can sort data based on a column in the table in ascending/descending order
* Manage accounts
  * Create new SQL account
  * Delete existing SQL account
  * Modify the privilege of an existing SQL account
* Three account privilege levels
  * Admin account
    * Can manage users
    * Can write to SQL database
    * Can read from SQL database
  * ReadAndWrite account
    * Can write to SQL database
    * Can read from SQL database
  * ReadOnly account
    * Can read from SQL database
