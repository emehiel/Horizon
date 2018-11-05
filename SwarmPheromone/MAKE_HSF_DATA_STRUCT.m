function [] = MAKE_HSF_DATA_STRUCT

% swarm size to load
assets = 4;

path = ['E:\Horizon Swarm Data\swarm_', num2str(assets),'\target\'];
delim = ',';
header = 1;
missing_state = {};
missing_fuel = {};
missing_dist = {};
missing_found = {};


for i = 2:8:18 % update
    if i == 2
        for j = 2:8:18 % deposit
            if j == 2
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_02.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_02.Threshold_02.lat{:,m} = -0.00035 + temp_state.data(:,2);
                            State.Update_02.Deposit_02.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_02.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_02.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_02.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_02.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_02.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_02.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_02.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_02.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_02.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_02.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_02.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_02.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_02.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_02.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_02.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_02.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_02.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_02.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_02.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_02.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_02.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_02.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_02.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_02.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_02.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_02.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_02.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_02.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_02.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_02.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_02.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_02.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_02.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_02.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_02.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_02.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_02.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_02.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_02.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_02.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_02.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_02.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_02.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_02.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_02.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_02.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_02.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_02.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_02.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_02.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_02.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_02.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_02.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_02.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_02.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_02.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_02.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
            
            if j == 10
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_10.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_10.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_10.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_10.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_10.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_10.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_10.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_10.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_10.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_10.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_10.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_10.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_10.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_10.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_10.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_10.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_10.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_10.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_10.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_10.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_10.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_10.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_10.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_10.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_10.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_10.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_10.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_10.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_10.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_10.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_10.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_10.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_10.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_10.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_10.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_10.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_10.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_10.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_10.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_10.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_10.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_10.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_10.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_10.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_10.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_10.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_10.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_10.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_10.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_10.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_10.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_10.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_10.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_10.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_10.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_10.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_10.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_10.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_10.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_10.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
            
            if j == 18
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_18.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_18.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_18.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_18.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_18.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_18.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_18.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_18.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_18.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_18.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_18.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_18.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_18.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_18.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_18.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_18.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_18.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_18.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_18.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_18.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_18.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_18.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_18.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_18.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_18.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_18.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_18.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_18.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_18.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_18.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_18.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_18.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_18.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_18.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_18.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_18.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_18.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_18.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_18.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_18.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_02.Deposit_18.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_02.Deposit_18.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_02.Deposit_18.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_02.Deposit_18.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_02.Deposit_18.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_02.Deposit_18.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_02.Deposit_18.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_02.Deposit_18.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_02.Deposit_18.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_02.Deposit_18.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_02.Deposit_18.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_02.Deposit_18.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_02.Deposit_18.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_02.Deposit_18.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_02.Deposit_18.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_02.Deposit_18.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_02.Deposit_18.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_02.Deposit_18.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_02.Deposit_18.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_02.Deposit_18.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
        end
    end
    
    if i == 10
        for j = 2:8:18 % deposit
            if j == 2
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_02.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_02.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_02.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_02.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_02.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_02.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_02.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_02.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_02.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_02.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_02.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_02.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_02.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_02.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_02.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_02.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_02.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_02.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_02.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_02.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_02.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_02.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_02.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_02.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_02.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_02.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_02.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_02.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_02.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_02.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_02.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_02.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_02.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_02.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_02.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_02.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_02.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_02.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_02.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_02.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_02.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_02.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_02.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_02.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_02.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_02.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_02.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_02.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_02.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_02.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_02.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_02.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_02.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_02.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_02.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_02.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_02.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_02.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_02.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_02.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
            
            if j == 10
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_10.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_10.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_10.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_10.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_10.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_10.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_10.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_10.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_10.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_10.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_10.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_10.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_10.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_10.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_10.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_10.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_10.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_10.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_10.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_10.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_10.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_10.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_10.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_10.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_10.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_10.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_10.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_10.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_10.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_10.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_10.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_10.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_10.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_10.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_10.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_10.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_10.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_10.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_10.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_10.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_10.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_10.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_10.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_10.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_10.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_10.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_10.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_10.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_10.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_10.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_10.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_10.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_10.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_10.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_10.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_10.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_10.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_10.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_10.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_10.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
            
            if j == 18
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_18.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_18.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_18.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_18.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_18.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_18.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_18.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_18.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_18.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_18.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_18.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_18.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_18.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_18.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_18.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_18.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_18.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_18.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_18.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_18.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_18.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_18.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_18.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_18.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_18.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_18.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_18.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_18.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_18.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_18.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_18.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_18.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_18.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_18.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_18.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_18.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_18.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_18.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_18.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_18.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_10.Deposit_18.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_10.Deposit_18.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_10.Deposit_18.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_10.Deposit_18.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_10.Deposit_18.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_10.Deposit_18.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_10.Deposit_18.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_10.Deposit_18.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_10.Deposit_18.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_10.Deposit_18.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_10.Deposit_18.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_10.Deposit_18.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_10.Deposit_18.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_10.Deposit_18.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_10.Deposit_18.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_10.Deposit_18.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_10.Deposit_18.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_10.Deposit_18.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_10.Deposit_18.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_10.Deposit_18.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
        end
    end
    
    if i == 18
        for j = 2:8:18 % deposit
            if j == 2
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_02.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_02.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_02.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_02.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_02.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_02.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_02.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_02.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_02.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_02.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_02.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_02.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_02.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_02.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_02.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_02.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_02.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_02.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_02.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_02.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_02.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_02.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_02.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_02.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_02.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_02.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_02.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_02.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_02.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_02.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_02.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_02.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_02.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_02.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_02.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_02.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_02.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_02.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_02.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_02.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_02.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_02.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_02.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_02.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_02.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_02.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_02.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_02.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_02.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_02.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_02.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_02.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_02.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_02.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_02.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_02.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_02.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_02.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_02.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_02.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
            
            if j == 10
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_10.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_10.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_10.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_10.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_10.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_10.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_10.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_10.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_10.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_10.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_10.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_10.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_10.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_10.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_10.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_10.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_10.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_10.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_10.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_10.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_10.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_10.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_10.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_10.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_10.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_10.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_10.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_10.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_10.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_10.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_10.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_10.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_10.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_10.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_10.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_10.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_10.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_10.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_10.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_10.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_10.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_10.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_10.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_10.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_10.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_10.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_10.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_10.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_10.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_10.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_10.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_10.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_10.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_10.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_10.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_10.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_10.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_10.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_10.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_10.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
            
            if j == 18
                for k = 2:8:18 % threshold
                    if k == 2
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_18.Threshold_02.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_18.Threshold_02.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_18.Threshold_02.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_18.Threshold_02.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_18.Threshold_02.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_18.Threshold_02.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_18.Threshold_02.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_18.Threshold_02.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_18.Threshold_02.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_18.Threshold_02.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_18.Threshold_02.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_18.Threshold_02.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_18.Threshold_02.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_18.Threshold_02.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_18.Threshold_02.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_18.Threshold_02.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_18.Threshold_02.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_18.Threshold_02(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_18.Threshold_02(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_18.Threshold_02(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 10
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_18.Threshold_10.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_18.Threshold_10.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_18.Threshold_10.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_18.Threshold_10.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_18.Threshold_10.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_18.Threshold_10.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_18.Threshold_10.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_18.Threshold_10.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_18.Threshold_10.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_18.Threshold_10.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_18.Threshold_10.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_18.Threshold_10.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_18.Threshold_10.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_18.Threshold_10.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_18.Threshold_10.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_18.Threshold_10.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_18.Threshold_10.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_18.Threshold_10(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_18.Threshold_10(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_18.Threshold_10(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                    
                    if k == 18
                        for m = 1:assets % asset number
                        csv_state = [path,'asset',num2str(m),'_dynamicStateData_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv']; 
                        csv_fuel = [path,'asset',num2str(m),'_fuel_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_dist = [path,'asset',num2str(m),'_targetdistance_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        csv_found = [path,'asset',num2str(m),'_targetfound_up',num2str(i),'_dep',num2str(j),'_thr',num2str(k),'.csv'];
                        
                        try
                            temp_state = importdata(csv_state, delim, header);
                            State.Update_18.Deposit_18.Threshold_18.t{:,m} = temp_state.data(:,1);
                            State.Update_18.Deposit_18.Threshold_18.lat{:,m} = temp_state.data(:,2);
                            State.Update_18.Deposit_18.Threshold_18.lon{:,m} = -0.0023 - temp_state.data(:,3);
                            State.Update_18.Deposit_18.Threshold_18.alt{:,m} = temp_state.data(:,4);
                            State.Update_18.Deposit_18.Threshold_18.vx{:,m} = 36 + temp_state.data(:,5);
                            State.Update_18.Deposit_18.Threshold_18.vy{:,m} = -temp_state.data(:,6);
                            State.Update_18.Deposit_18.Threshold_18.vz{:,m} = temp_state.data(:,7);
                            State.Update_18.Deposit_18.Threshold_18.p{:,m} = -temp_state.data(:,8);
                            State.Update_18.Deposit_18.Threshold_18.q{:,m} = temp_state.data(:,9);
                            State.Update_18.Deposit_18.Threshold_18.r{:,m} = temp_state.data(:,10);
                            State.Update_18.Deposit_18.Threshold_18.N{:,m} = temp_state.data(:,11);
                            State.Update_18.Deposit_18.Threshold_18.E{:,m} = temp_state.data(:,12);
                            State.Update_18.Deposit_18.Threshold_18.D{:,m} = temp_state.data(:,13);
                            State.Update_18.Deposit_18.Threshold_18.phi{:,m} = temp_state.data(:,14);
                            State.Update_18.Deposit_18.Threshold_18.theta{:,m} = temp_state.data(:,15);
                            State.Update_18.Deposit_18.Threshold_18.psi{:,m} = temp_state.data(:,16);
                            State.Update_18.Deposit_18.Threshold_18.tau{:,m} = temp_state.data(:,17);
                        catch
                            missing_state = [missing_state; {csv_state}];
                        end
                        try
                            temp_fuel = importdata(csv_fuel, delim, header);
                            Fuel.Update_18.Deposit_18.Threshold_18(:,:,m) = temp_fuel.data;
                        catch
                            missing_fuel = [missing_fuel; {csv_fuel}];
                        end
                        try
                            temp_dist = importdata(csv_dist, delim, header);
                            Distance.Update_18.Deposit_18.Threshold_18(:,:,m) = temp_dist.data;
                        catch
                            missing_dist = [missing_dist; {csv_dist}];
                        end
                        try
                            temp_found = importdata(csv_found, delim, header);
                            Found.Update_18.Deposit_18.Threshold_18(:,:,m) = temp_found.data;
                        catch
                            missing_found = [missing_found; {csv_found}];
                        end
                        end
                    end
                end
            end
        end
    end
end

%% Create data structure based on swarm size
if assets == 1
    swarm_1.State = State;
    swarm_1.Fuel = Fuel;
    swarm_1.Distance = Distance;
    swarm_1.Found = Found;
    save('swarm_1.mat','swarm_1','missing_state','missing_fuel','missing_dist','missing_found','-v7.3')
end
if assets == 2
    swarm_2.State = State;
    swarm_2.Fuel = Fuel;
    swarm_2.Distance = Distance;
    swarm_2.Found = Found;
    save('swarm_2.mat','swarm_2','missing_state','missing_fuel','missing_dist','missing_found','-v7.3')
end
if assets == 3
    swarm_3.State = State;
    swarm_3.Fuel = Fuel;
    swarm_3.Distance = Distance;
    swarm_3.Found = Found;
    save('swarm_3.mat','swarm_3','missing_state','missing_fuel','missing_dist','missing_found','-v7.3')
end
if assets == 4
    swarm_4.State = State;
    swarm_4.Fuel = Fuel;
    swarm_4.Distance = Distance;
    swarm_4.Found = Found;
    save('swarm_4.mat','swarm_4','missing_state','missing_fuel','missing_dist','missing_found','-v7.3')
end