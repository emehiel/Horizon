function [] = GENERATE_LATEX_TABLE_DATA(swarm, num)

fid = fopen(['swarm_',num2str(num),'_table.txt'],'w');

if num ==1 
for i = 2:8:18
    update = i;
    
    for j = 2:8:18
        deposit = j;
        
        for k = 2:8:18
            if k == 2
                eval(['x = swarm.Distance.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k)])
                idx = find(x(:,1) >= 260);
                mean_x = mean(x(idx,2));
                max_x = max(x(idx,2));

                eval(['y = swarm.Fuel.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k)])

                th1_max = num2str( max_x, '%4.2f');
                th1_mean = num2str( mean_x, '%4.2f');
                th1_fuel = num2str( sum(y(:,2)), '%2.4f');
            elseif k == 10
                eval(['x = swarm.Distance.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k)])
                idx = find(x(:,1) >= 260);
                mean_x = mean(x(idx,2));
                %max_idx = find( x == max(x(idx,2)) );

                eval(['y = swarm.Fuel.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k)])

                th2_max = num2str( max(x(idx,2)), '%4.2f');
                th2_mean = num2str( mean_x, '%4.2f');
                th2_fuel = num2str( sum(y(:,2)), '%2.4f');
            elseif k == 18
                eval(['x = swarm.Distance.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k)])
                idx = find(x(:,1) >= 260);
                mean_x = mean(x(idx,2));
                %max_idx = find( x == max(x(idx,2)) );

                eval(['y = swarm.Fuel.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k)])

                th3_max = num2str( max(x(idx,2)), '%4.2f');
                th3_mean = num2str( mean_x, '%4.2f');
                th3_fuel = num2str( sum(y(:,2)), '%2.4f');
            end
        end
        fprintf(fid, '%i &%i &Max distance &%s m &%s m &%s m \n', update, deposit, th1_max, th2_max, th3_max);
        fprintf(fid, '%i &%i &Mean distance &%s m &%s m &%s m \n', update, deposit, th1_mean, th2_mean, th3_mean);
        fprintf(fid, '%i &%i &Fuel consumed &%s gal &%s gal &%s gal \n', update, deposit, th1_fuel, th2_fuel, th3_fuel);
    end
end



else
for i = 2:8:18
    update = i;
    
    for j = 2:8:18
        deposit = j;
        
        for k = 2:8:18
            if k == 2
                for m = 1:num
                    eval(['x = swarm.Distance.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k),';']);
                    idx = find(x(:,1) >= 260);
                    %mean_x = mean2(x(idx,2,m));
                    %max_x = max(x(idx,2,m));
                    mean_x = mean2(x(idx,2,:));
                    max_x = max(max(x(idx,2,:)));

                    eval(['y = swarm.Fuel.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k),';']);

                    eval(['th1',num2str(m),'_max = num2str( max_x, ''%4.2f'');']);
                    eval(['th1',num2str(m),'_mean = num2str( mean_x, ''%4.2f'');']);
                    eval(['th1',num2str(m),'_fuel = num2str( sum(y(:,2,m)), ''%2.4f'');']);
                end
            elseif k == 10
                for m = 1:num
                    eval(['x = swarm.Distance.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k),';']);
                    idx = find(x(:,1) >= 260);
                    %mean_x = mean(x(idx,2,m));
                    %max_x = max(x(idx,2,m));
                    mean_x = mean2(x(idx,2,:));
                    max_x = max(max(x(idx,2,m)));
                    %max_idx = find( x == max(x(idx,2)) );

                    eval(['y = swarm.Fuel.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k),';']);

                    eval(['th2',num2str(m),'_max = num2str( max_x, ''%4.2f'');']);
                    eval(['th2',num2str(m),'_mean = num2str( mean_x, ''%4.2f'');']);
                    eval(['th2',num2str(m),'_fuel = num2str( sum(y(:,2,m)), ''%2.4f'');']);
                end
            elseif k == 18
                for m = 1:num
                    eval(['x = swarm.Distance.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k),';']);
                    idx = find(x(:,1) >= 260);
                    %mean_x = mean(x(idx,2,m));
                    %max_x = max(x(idx,2,m));
                    mean_x = mean2(x(idx,2,:));
                    max_x = max(max(x(idx,2,m)));
                    %max_idx = find( x == max(x(idx,2)) );

                    eval(['y = swarm.Fuel.Update_',sprintf('%02d', i),'.Deposit_',sprintf('%02d', j),'.Threshold_',sprintf('%02d', k),';']);

                    eval(['th3',num2str(m),'_max = num2str( max_x, ''%4.2f'');']);
                    eval(['th3',num2str(m),'_mean = num2str( mean_x, ''%4.2f'');']);
                    eval(['th3',num2str(m),'_fuel = num2str( sum(y(:,2,m)), ''%2.4f'');']);
                end
            end
        end
        if num == 2
            fprintf(fid, '%i &%i &1 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th11_max, th21_max, th31_max);
            fprintf(fid, '%i &%i &1 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th11_mean, th21_mean, th31_mean);
            fprintf(fid, '%i &%i &1 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th11_fuel, th21_fuel, th31_fuel);
            fprintf(fid, '%i &%i &2 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th12_max, th22_max, th32_max);
            fprintf(fid, '%i &%i &2 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th12_mean, th22_mean, th32_mean);
            fprintf(fid, '%i &%i &2 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th12_fuel, th22_fuel, th32_fuel);
        elseif num == 3
            fprintf(fid, '%i &%i &1 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th11_max, th21_max, th31_max);
            fprintf(fid, '%i &%i &1 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th11_mean, th21_mean, th31_mean);
            fprintf(fid, '%i &%i &1 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th11_fuel, th21_fuel, th31_fuel);
            fprintf(fid, '%i &%i &2 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th12_max, th22_max, th32_max);
            fprintf(fid, '%i &%i &2 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th12_mean, th22_mean, th32_mean);
            fprintf(fid, '%i &%i &2 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th12_fuel, th22_fuel, th32_fuel);
            fprintf(fid, '%i &%i &3 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th13_max, th23_max, th33_max);
            fprintf(fid, '%i &%i &3 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th13_mean, th23_mean, th33_mean);
            fprintf(fid, '%i &%i &3 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th13_fuel, th23_fuel, th33_fuel);
        elseif num == 4
            fprintf(fid, '%i &%i &1 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th11_max, th21_max, th31_max);
            fprintf(fid, '%i &%i &1 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th11_mean, th21_mean, th31_mean);
            fprintf(fid, '%i &%i &1 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th11_fuel, th21_fuel, th31_fuel);
            fprintf(fid, '%i &%i &2 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th12_max, th22_max, th32_max);
            fprintf(fid, '%i &%i &2 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th12_mean, th22_mean, th32_mean);
            fprintf(fid, '%i &%i &2 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th12_fuel, th22_fuel, th32_fuel);
            fprintf(fid, '%i &%i &3 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th13_max, th23_max, th33_max);
            fprintf(fid, '%i &%i &3 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th13_mean, th23_mean, th33_mean);
            fprintf(fid, '%i &%i &3 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th13_fuel, th23_fuel, th33_fuel);
            fprintf(fid, '%i &%i &4 &Max distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th14_max, th24_max, th34_max);
            fprintf(fid, '%i &%i &4 &Mean distance &%s m &%s m &%s m \\\\ \\hline \n', update, deposit, th14_mean, th24_mean, th34_mean);
            fprintf(fid, '%i &%i &4 &Fuel consumed &%s gal &%s gal &%s gal \\\\ \\hline \n', update, deposit, th14_fuel, th24_fuel, th34_fuel);
        end
    end
end

end
fclose(fid);
%swarm size = 1
%fprintf(fid, '%i &%i &%s &%s m &%s m &%s m \n', update, deposit, param, th1, th2, th3);

%swarm size > 1
%fprintf(fid, '%i &%i &%i &%s m &%s m &%s m \n', update, deposit, param, th1, th2, th3);