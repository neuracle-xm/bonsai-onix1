function data = readOnixData(path)
    fid = fopen(path, 'rb');
    [X,~] = fread(fid,'float32');
    fclose(fid);
    data = reshape(X,64,[]);
end