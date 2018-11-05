close all
clear
clc

%% Choose data to load
load_swarm_1 = 0;
load_swarm_2 = 1;
load_swarm_3 = 0;
load_swarm_4 = 0;
generate_file_1 = 0;
generate_file_2 = 0;
generate_file_3 = 0;
generate_file_4 = 0;


%% Load data structures
if load_swarm_1
    % control case only
    load('E:\Horizon Swarm Data\swarm_1.mat');
end
if load_swarm_2
    % control case and test case
    load('E:\Horizon Swarm Data\swarm_1.mat');
    load('E:\Horizon Swarm Data\swarm_2.mat');
end
if load_swarm_3
    % control case and test case
    load('E:\Horizon Swarm Data\swarm_1.mat');
    load('E:\Horizon Swarm Data\swarm_3.mat');
end
if load_swarm_4
    % control case and test case
    load('E:\Horizon Swarm Data\swarm_1.mat');
    load('E:\Horizon Swarm Data\swarm_4.mat');
end
if generate_file_1
    load('E:\Horizon Swarm Data\swarm_1.mat');
end
if generate_file_2
    load('E:\Horizon Swarm Data\swarm_2.mat');
end
if generate_file_3
    load('E:\Horizon Swarm Data\swarm_3.mat');
end
if generate_file_4
    load('E:\Horizon Swarm Data\swarm_4.mat');
end


%% Plot distance data

if load_swarm_1
    
    %figure()
    %PLOT_CONTROL_MEAN_DISTANCE(swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,1), swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,2));
    %figure()
    %PLOT_CONTROL_MEAN_DISTANCE(swarm_1.Distance.Update_18.Deposit_18.Threshold_18(:,1), swarm_1.Distance.Update_18.Deposit_18.Threshold_18(:,2));
    
    PLOT_HSF_NAV(swarm_1.State.Update_02.Deposit_02.Threshold_02.lon{1}, swarm_1.State.Update_02.Deposit_02.Threshold_02.lat{1});
    PLOT_HSF_NAV(swarm_1.State.Update_18.Deposit_18.Threshold_18.lon{1}, swarm_1.State.Update_18.Deposit_18.Threshold_18.lat{1});
    
    PLOT_FUEL(swarm_1.Fuel.Update_02.Deposit_02.Threshold_02(:,1), swarm_1.Fuel.Update_02.Deposit_02.Threshold_02(:,2));
    PLOT_FUEL(swarm_1.Fuel.Update_18.Deposit_18.Threshold_18(:,1), swarm_1.Fuel.Update_18.Deposit_18.Threshold_18(:,2));
    
    PLOT_UAV_STATE(swarm_1.State.Update_02.Deposit_02.Threshold_02, 1);
    PLOT_UAV_STATE(swarm_1.State.Update_18.Deposit_18.Threshold_18, 1);
    
end
if load_swarm_2
  
    PLOT_MEAN_DISTANCE(swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,1), swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,2), ...
        swarm_2.Distance.Update_02.Deposit_02.Threshold_02(:,1,1), swarm_2.Distance.Update_02.Deposit_02.Threshold_02(:,2,1), ...
        swarm_2.Distance.Update_02.Deposit_02.Threshold_02(:,1,2), swarm_2.Distance.Update_02.Deposit_02.Threshold_02(:,2,2))
    PLOT_MEAN_DISTANCE(swarm_1.Distance.Update_18.Deposit_18.Threshold_10(:,1), swarm_1.Distance.Update_18.Deposit_18.Threshold_10(:,2), ...
        swarm_2.Distance.Update_18.Deposit_18.Threshold_10(:,1,1), swarm_2.Distance.Update_18.Deposit_02.Threshold_10(:,2,1), ...
        swarm_2.Distance.Update_18.Deposit_18.Threshold_10(:,1,2), swarm_2.Distance.Update_18.Deposit_18.Threshold_10(:,2,2))
    
    PLOT_HSF_NAV(swarm_2.State.Update_02.Deposit_02.Threshold_02.lon{1}, swarm_2.State.Update_02.Deposit_02.Threshold_02.lat{1}, ...
        swarm_2.State.Update_02.Deposit_02.Threshold_02.lon{2}, swarm_2.State.Update_02.Deposit_02.Threshold_02.lat{2});
    PLOT_HSF_NAV(swarm_2.State.Update_18.Deposit_18.Threshold_10.lon{1}, swarm_2.State.Update_18.Deposit_18.Threshold_10.lat{1}, ...
        swarm_2.State.Update_18.Deposit_18.Threshold_10.lon{2}, swarm_2.State.Update_18.Deposit_18.Threshold_10.lat{2});
end
if load_swarm_3
    
    PLOT_MEAN_DISTANCE(swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,1), swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,2), ...
        swarm_3.Distance.Update_02.Deposit_02.Threshold_02(:,1,1), swarm_3.Distance.Update_02.Deposit_02.Threshold_02(:,2,1), ...
        swarm_3.Distance.Update_02.Deposit_02.Threshold_02(:,1,2), swarm_3.Distance.Update_02.Deposit_02.Threshold_02(:,2,2), ...
        swarm_3.Distance.Update_02.Deposit_02.Threshold_02(:,1,3), swarm_3.Distance.Update_02.Deposit_02.Threshold_02(:,2,3))
    PLOT_MEAN_DISTANCE(swarm_1.Distance.Update_18.Deposit_18.Threshold_18(:,1), swarm_1.Distance.Update_18.Deposit_18.Threshold_18(:,2), ...
        swarm_3.Distance.Update_18.Deposit_18.Threshold_18(:,1,1), swarm_3.Distance.Update_18.Deposit_18.Threshold_18(:,2,1), ...
        swarm_3.Distance.Update_18.Deposit_18.Threshold_18(:,1,2), swarm_3.Distance.Update_18.Deposit_18.Threshold_18(:,2,2), ...
        swarm_3.Distance.Update_18.Deposit_18.Threshold_18(:,1,3), swarm_3.Distance.Update_18.Deposit_18.Threshold_18(:,2,3))
    
    PLOT_HSF_NAV(swarm_3.State.Update_02.Deposit_02.Threshold_02.lon{1}, swarm_3.State.Update_02.Deposit_02.Threshold_02.lat{1}, ...
        swarm_3.State.Update_02.Deposit_02.Threshold_02.lon{2}, swarm_3.State.Update_02.Deposit_02.Threshold_02.lat{2}, ...
        swarm_3.State.Update_02.Deposit_02.Threshold_02.lon{3}, swarm_3.State.Update_02.Deposit_02.Threshold_02.lat{3});
    PLOT_HSF_NAV(swarm_3.State.Update_18.Deposit_18.Threshold_18.lon{1}, swarm_3.State.Update_18.Deposit_18.Threshold_18.lat{1}, ...
        swarm_3.State.Update_18.Deposit_18.Threshold_18.lon{2}, swarm_3.State.Update_18.Deposit_18.Threshold_18.lat{2}, ...
        swarm_3.State.Update_18.Deposit_18.Threshold_18.lon{3}, swarm_3.State.Update_18.Deposit_18.Threshold_18.lat{3});
end
if load_swarm_4
    
    PLOT_MEAN_DISTANCE(swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,1), swarm_1.Distance.Update_02.Deposit_02.Threshold_02(:,2), ...
        swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,1,1), swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,2,1), ...
        swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,1,2), swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,2,2), ...
        swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,1,3), swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,2,3), ...
        swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,1,4), swarm_4.Distance.Update_02.Deposit_02.Threshold_02(:,2,4))
    PLOT_MEAN_DISTANCE(swarm_1.Distance.Update_18.Deposit_18.Threshold_10(:,1), swarm_1.Distance.Update_18.Deposit_18.Threshold_10(:,2), ...
        swarm_4.Distance.Update_18.Deposit_18.Threshold_18(:,1,1), swarm_4.Distance.Update_18.Deposit_18.Threshold_10(:,2,1), ...
        swarm_4.Distance.Update_18.Deposit_18.Threshold_18(:,1,2), swarm_4.Distance.Update_18.Deposit_18.Threshold_10(:,2,2), ...
        swarm_4.Distance.Update_18.Deposit_18.Threshold_18(:,1,3), swarm_4.Distance.Update_18.Deposit_18.Threshold_10(:,2,3), ...
        swarm_4.Distance.Update_18.Deposit_18.Threshold_18(:,1,4), swarm_4.Distance.Update_18.Deposit_18.Threshold_10(:,2,4))
    
    PLOT_HSF_NAV(swarm_4.State.Update_02.Deposit_02.Threshold_02.lon{1}, swarm_4.State.Update_02.Deposit_02.Threshold_02.lat{1}, ...
        swarm_4.State.Update_02.Deposit_02.Threshold_02.lon{2}, swarm_4.State.Update_02.Deposit_02.Threshold_02.lat{2}, ...
        swarm_4.State.Update_02.Deposit_02.Threshold_02.lon{3}, swarm_4.State.Update_02.Deposit_02.Threshold_02.lat{3}, ...
        swarm_4.State.Update_02.Deposit_02.Threshold_02.lon{4}, swarm_4.State.Update_02.Deposit_02.Threshold_02.lat{4});
    PLOT_HSF_NAV(swarm_4.State.Update_18.Deposit_18.Threshold_10.lon{1}, swarm_4.State.Update_18.Deposit_18.Threshold_10.lat{1}, ...
        swarm_4.State.Update_18.Deposit_18.Threshold_10.lon{2}, swarm_4.State.Update_18.Deposit_18.Threshold_10.lat{2}, ...
        swarm_4.State.Update_18.Deposit_18.Threshold_10.lon{3}, swarm_4.State.Update_18.Deposit_18.Threshold_10.lat{3}, ...
        swarm_4.State.Update_18.Deposit_18.Threshold_10.lon{4}, swarm_4.State.Update_18.Deposit_18.Threshold_10.lat{4});
end

if generate_file_1
    GENERATE_LATEX_TABLE_DATA(swarm_1, 1)
end
if generate_file_2
    GENERATE_LATEX_TABLE_DATA(swarm_2, 2)
end
if generate_file_3
    GENERATE_LATEX_TABLE_DATA(swarm_3, 3)
end
if generate_file_4
    GENERATE_LATEX_TABLE_DATA(swarm_4, 4)
end
