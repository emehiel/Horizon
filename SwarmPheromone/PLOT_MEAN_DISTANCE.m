function [] = PLOT_MEAN_DISTANCE(xc, yc, varargin)

switch nargin
    case 6
        x1 = varargin{1};
        y1 = varargin{2};
        x2 = varargin{3};
        y2 = varargin{4};
        
        idxc = find(xc >= 260);
        idx1 = find(x1 >= 260);
        idx2 = find(x2 >= 260);
        
        data = [y1(idx1); y2(idx2)];
        mean_y = mean(data);
        max_1 = max(y1(idx1));
        max_2 = max(y2(idx2));
        if max_1 > max_2
            max_idx = find( y1 == max_1 );
            max_val = max_1;
        else
            max_idx = find( y2 == max_2 );
            max_val = max_2;
        end

        disp(['Average Distance: ',num2str(mean_y),' meters'])
        disp(['Maximum Distance: ',num2str(max_val),' meters'])
        
        figure()
        plot( xc(idxc), yc(idxc), '--k','linewidth',0.75 ); hold on
        plot( x1(idx1), y1(idx1) );
        plot( x2(idx2), y2(idx2) ); grid on
        plot( [260 800],[mean_y mean_y],'--r','linewidth',1 )
        plot( xc(max_idx), max_val, 'ro','linewidth',0.75 )
        legend('base case','asset 1','asset 2','swarm avg','max')
        xlim([260 800])
        
    case 8
        x1 = varargin{1};
        y1 = varargin{2};
        x2 = varargin{3};
        y2 = varargin{4};
        x3 = varargin{5};
        y3 = varargin{6};
        
        idxc = find(xc >= 260);
        idx1 = find(x1 >= 260);
        idx2 = find(x2 >= 260);
        idx3 = find(x3 >= 260);
        
        data = [y1(idx1); y2(idx2); y3(idx3)];
        mean_y = mean(data);
        max_1 = max(y1(idx1));
        max_2 = max(y2(idx2));
        max_3 = max(y3(idx3));
        if max_1 > max_2 && max_1 > max_3
            max_idx = find( y1 == max_1 );
            max_val = max_1;
        elseif max_2 > max_1 && max_2 > max_3
            max_idx = find( y2 == max_2 );
            max_val = max_2;
        elseif max_3 > max_1 && max_3 > max_2
            max_idx = find( y3 == max_3 );
            max_val = max_3;
        end

        disp(['Average Distance: ',num2str(mean_y),' meters'])
        disp(['Maximum Distance: ',num2str(max(data)),' meters'])
        
        figure()
        plot( xc(idxc), yc(idxc), '--k','linewidth',0.75 ); hold on
        plot( x1(idx1), y1(idx1) );
        plot( x2(idx2), y2(idx2) );
        plot( x3(idx3), y3(idx3) ); grid on
        plot( [260 800],[mean_y mean_y],'--r','linewidth',1 )
        plot( xc(max_idx), max_val, 'ro','linewidth',0.75 )
        legend('base case','asset 1','asset 2','asset 3','swarm avg','max')
        xlim([260 800])
        
    case 10
        x1 = varargin{1};
        y1 = varargin{2};
        x2 = varargin{3};
        y2 = varargin{4};
        x3 = varargin{5};
        y3 = varargin{6};
        x4 = varargin{7};
        y4 = varargin{8};
        
        idxc = find(xc >= 260);
        idx1 = find(x1 >= 260);
        idx2 = find(x2 >= 260);
        idx3 = find(x3 >= 260);
        idx4 = find(x4 >= 270);
        
        data = [y1(idx1); y2(idx2); y3(idx3); y4(idx4)];
        mean_y = mean(data);
        max_1 = max(y1(idx1));
        max_2 = max(y2(idx2));
        max_3 = max(y3(idx3));
        max_4 = max(y4(idx4));
        if max_1 > max_2 && max_1 > max_3 && max_1 > max_4
            max_idx = find( y1 == max_1 );
            max_val = max_1;
        elseif max_2 > max_1 && max_2 > max_3 && max_2 > max_4
            max_idx = find( y2 == max_2 );
            max_val = max_2;
        elseif max_3 > max_1 && max_3 > max_2 && max_3 > max_4
            max_idx = find( y3 == max_3 );
            max_val = max_3;
        elseif max_4 > max_1 && max_4 > max_2 && max_4 > max_3
            max_idx = find( y4 == max_4 );
            max_val = max_4;
        end

        disp(['Average Distance: ',num2str(mean_y),' meters'])
        disp(['Maximum Distance: ',num2str(max(data)),' meters'])
        
        figure()
        plot( xc(idxc), yc(idxc), '--k','linewidth',0.75 ); hold on
        plot( x1(idx1), y1(idx1) );
        plot( x2(idx2), y2(idx2) );
        plot( x3(idx3), y3(idx3) );
        plot( x4(idx4), y3(idx4) ); grid on
        plot( [260 800],[mean_y mean_y],'--r','linewidth',1 )
        plot( xc(max_idx), max_val, 'ro','linewidth',0.75 )
        legend('base case','asset 1','asset 2','asset 3','asset 4','swarm avg','max')
        xlim([260 800])
        
    otherwise
        error("incorrect argument input number")
end

xlabel('Time (s)')
ylabel('Distance (m)')